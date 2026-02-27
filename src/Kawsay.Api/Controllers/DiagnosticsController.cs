using Application.DTOs;
using Application.Interfaces.Infrastructure;
using Application.Models.Solver;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/solver/diagnostics")]
public class DiagnosticsController(ISolverClient solverClient) : ControllerBase
{
    /// <summary>
    /// DIRECT DIAGNOSTIC PORT: Accepts aftermarket JSON from the OBD Scanner,
    /// adapts it to OEM Entities, and fires it into the gRPC pipeline.
    /// </summary>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(GeneratedTimetableDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Simulate([FromBody] DiagnosticRomPayload payload, CancellationToken cancellationToken)
    {
        // 1. The OBD Adapter: Translate lightweight JSON into heavy EF Entities
        var context = new SchedulingContext 
        { 
            JobId = payload.JobId,
            Timetable = new TimetableEntity
            {
                Id = payload.Timetable.Id,
                Days = payload.Timetable.Days.Select(d => new TimetableDayEntity { Id = d }).ToList(),
                Periods = payload.Timetable.Periods.Select(p => new TimetablePeriodEntity { Id = p }).ToList()
            }
        };

        foreach (var c in payload.Cohorts)
        {
            var cohort = new CohortEntity { Id = ParseStringId(c.Id) };
            foreach (var sg in c.StudentGroups)
            {
                cohort.StudentGroups.Add(new StudentGroupEntity { Id = ParseStringId(sg.Id), Name = sg.Name });
            }
            context.Cohorts.Add(cohort);
        }

        foreach (var r in payload.Requirements)
        {
            var reqEntity = new CourseRequirementEntity
            {
                Id = r.Id,
                TimetableId = r.TimetableId,
                CourseId = 1, // Dummy ID, Solver only cares about the Code
                Course = new CourseEntity { Code = r.Course.Code },
                DurationInPeriods = r.DurationInPeriods,
                FrequencyPerWeek = r.FrequencyPerWeek,
                PreferredTeacherId = string.IsNullOrEmpty(r.PreferredTeacherId) ? null : ParseStringId(r.PreferredTeacherId)
            };

            // Adapter logic: The ROMs sometimes send a single ID, sometimes an array.
            // We grab the primary one to satisfy the production entity constraints.
            var primaryGroupId = r.StudentGroupId ?? r.StudentGroupIds?.FirstOrDefault();
            if (!string.IsNullOrEmpty(primaryGroupId))
            {
                reqEntity.StudentGroupId = ParseStringId(primaryGroupId);
            }

            context.Requirements.Add(reqEntity);
        }

        // 2. Fire directly into the Engine (gRPC)
        var result = await solverClient.SolveAsync(context, cancellationToken);
        
        if (result.IsFailure) 
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Error!.Message });

        // 3. Map the telemetry back to the standard DTO for the scanner HUD
        var dto = new GeneratedTimetableDto
        {
            JobId = result.Value!.JobId,
            Status = result.Value.Status,
            Message = result.Value.Message,
            Score = result.Value.QualityScore,
            Classes = result.Value.ScheduledItems.Select(item => new GeneratedClassDto
            {
                TempId = item.ReferenceId,
                RequirementId = ParseReqId(item.ReferenceId),
                DayIndex = item.DayIndex,
                StartPeriodIndex = item.StartSlotIndex,
                Duration = item.Duration
            }).ToList()
        };

        return Ok(dto);
    }

    private static int ParseReqId(string refId)
    {
        var parts = refId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var id)) return id;
        return 0;
    }

    // Helper: Safely converts UUID strings from the ROMs into integers for EF Core
    private static int ParseStringId(string id)
    {
        if (int.TryParse(id, out var parsed)) return parsed;
        return Math.Abs(id.GetHashCode()); 
    }
}

// --- OBD Adapter DTOs (Matches the Go Scanner JSON exactly) ---

public class DiagnosticRomPayload
{
    public string JobId { get; set; } = string.Empty;
    public DiagnosticTimetable Timetable { get; set; } = new();
    public List<DiagnosticCohort> Cohorts { get; set; } = new();
    public List<DiagnosticRequirement> Requirements { get; set; } = new();
}

public class DiagnosticTimetable
{
    public int Id { get; set; }
    public List<int> Days { get; set; } = new();
    public List<int> Periods { get; set; } = new();
}

public class DiagnosticCohort
{
    public string Id { get; set; } = string.Empty;
    public List<DiagnosticStudentGroup> StudentGroups { get; set; } = new();
}

public class DiagnosticStudentGroup
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class DiagnosticRequirement
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public DiagnosticCourse Course { get; set; } = new();
    public string? PreferredTeacherId { get; set; }
    public string? StudentGroupId { get; set; }
    public List<string>? StudentGroupIds { get; set; }
    public int FrequencyPerWeek { get; set; }
    public int DurationInPeriods { get; set; }
}

public class DiagnosticCourse
{
    public string Code { get; set; } = string.Empty;
}

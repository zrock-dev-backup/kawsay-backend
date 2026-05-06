using System.Diagnostics;
using Application.Core;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Application.Models.Solver;
using Domain.Entities;

namespace Application.Services;

public class TimetableGenerationService(
    ITimetableRepository timetableRepo,
    ICourseRequirementRepository requirementRepo,
    IRelationalMappingRepository mappingRepo,
    IStagedPlacementRepository stagedPlacementRepo,
    IUnitOfWork unitOfWork,
    ISolverClient solverClient)
{
    public async Task<Result<GeneratedTimetableDto>> GenerateTimetableAsync(int timetableId,
        CancellationToken cancellationToken = default)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("GenerateTimetable");
        activity?.SetTag(KawsayTelemetry.Attributes.TimetableId, timetableId);
        
        var timetable = await timetableRepo.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<GeneratedTimetableDto>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

        using var dataLoadActivity = KawsayTelemetry.ActivitySource.StartActivity("HydrateSolverContext");

        var requirements = await requirementRepo.GetByTimetableIdAsync(timetableId);
        var teacherIds = await mappingRepo.GetTeacherAssignmentsAsync(timetableId);
        var teacherAvailabilities = new List<TeacherAvailabilityEntity>();

        foreach (var tId in teacherIds)
        {
            var availabilities = await mappingRepo.GetTeacherAvailabilitiesAsync(timetableId, tId);
            teacherAvailabilities.AddRange(availabilities);
        }

        var context = new SchedulingContext
        {
            JobId = Guid.NewGuid().ToString(),
            Timetable = timetable,
            Requirements = requirements,
            AssignedTeacherIds = teacherIds,
            TeacherAvailabilities = teacherAvailabilities
        };

        activity?.SetTag(KawsayTelemetry.Attributes.ConstraintCount, context.Requirements.Count);

        var result = await solverClient.SolveAsync(context, cancellationToken);
        if (result.IsFailure)
        {
            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Message);
            return Result<GeneratedTimetableDto>.Failure(result.Error);
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            using var dbActivity = KawsayTelemetry.ActivitySource.StartActivity("PersistResults");
            await stagedPlacementRepo.DeleteAllForTimetableAsync(timetableId);

            var orderedDays = timetable.Days.OrderBy(d => d.Id).ToList();
            var orderedPeriods = timetable.Periods.OrderBy(p => p.Id).ToList();
            var dtoClasses = new List<GeneratedClassDto>();

            foreach (var item in result.Value!.ScheduledItems)
            {
                var reqId = ParseReqId(item.ReferenceId);
                if (item.DayIndex >= orderedDays.Count || item.StartSlotIndex >= orderedPeriods.Count) continue;

                var dayId = orderedDays[item.DayIndex].Id;
                var periodId = orderedPeriods[item.StartSlotIndex].Id;

                await stagedPlacementRepo.AddAsync(new StagedPlacementEntity
                {
                    CourseRequirementId = reqId,
                    DayId = dayId,
                    StartPeriodId = periodId,
                    Length = item.Duration
                });

                // ENRICHMENT
                var req = requirements.FirstOrDefault(r => r.Id == reqId);
                if (req == null)
                {
                    // Track warning in OpenTelemetry without needing ILogger
                    dbActivity?.AddEvent(new ActivityEvent($"Warning: Generated block {item.ReferenceId} matched no known requirement. Metadata omitted."));
                }

                dtoClasses.Add(new GeneratedClassDto
                {
                    TempId = item.ReferenceId,
                    RequirementId = reqId,
                    DayIndex = item.DayIndex,
                    StartPeriodIndex = item.StartSlotIndex,
                    Duration = item.Duration,
                    Activity = req == null ? null : new ActivitySummaryDto
                    {
                        SubjectId = req.SubjectId,
                        TeacherId = req.TeacherId,
                        StudentGroupId = req.StudentGroupId,
                        ClassType = req.ClassType,
                        FrequencyPerWeek = req.FrequencyPerWeek
                    }
                });
            }

            await unitOfWork.CommitTransactionAsync();

            return Result<GeneratedTimetableDto>.Success(new GeneratedTimetableDto
            {
                JobId = result.Value.JobId,
                Status = result.Value.Status,
                Message = result.Value.Message,
                Score = result.Value.QualityScore,
                Classes = dtoClasses
            });
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result<GeneratedTimetableDto>.Failure(Error.Failure("Persistence.Failed",
                $"Failed to save schedule: {ex.Message}"));
        }
    }

    public async Task<Result<List<GeneratedClassDto>>> GetScheduleAsync(int timetableId)
    {
        var timetable = await timetableRepo.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<List<GeneratedClassDto>>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

        var placements = await stagedPlacementRepo.GetByTimetableIdAsync(timetableId);
        var requirements = await requirementRepo.GetByTimetableIdAsync(timetableId);

        var orderedDays = timetable.Days.OrderBy(d => d.Id).ToList();
        var orderedPeriods = timetable.Periods.OrderBy(p => p.Id).ToList();

        var dtoClasses = new List<GeneratedClassDto>();

        foreach (var placement in placements)
        {
            var req = requirements.FirstOrDefault(r => r.Id == placement.CourseRequirementId);
            if (req == null) continue;

            var dayIndex = orderedDays.FindIndex(d => d.Id == placement.DayId);
            var periodIndex = orderedPeriods.FindIndex(p => p.Id == placement.StartPeriodId);

            if (dayIndex < 0 || periodIndex < 0) continue;

            dtoClasses.Add(new GeneratedClassDto
            {
                TempId = $"DB_REQ_{req.Id}_{placement.Id}",
                RequirementId = req.Id,
                DayIndex = dayIndex,
                StartPeriodIndex = periodIndex,
                Duration = placement.Length,
                Activity = new ActivitySummaryDto
                {
                    SubjectId = req.SubjectId,
                    TeacherId = req.TeacherId,
                    StudentGroupId = req.StudentGroupId,
                    ClassType = req.ClassType,
                    FrequencyPerWeek = req.FrequencyPerWeek
                }
            });
        }

        return Result<List<GeneratedClassDto>>.Success(dtoClasses);
    }

    private static int ParseReqId(string refId)
    {
        var parts = refId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var id)) return id;
        return 0;
    }
}

public class GeneratedTimetableDto
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Score { get; set; }
    public List<GeneratedClassDto> Classes { get; set; } = [];
}

public class GeneratedClassDto
{
    public string TempId { get; set; } = string.Empty;
    public int RequirementId { get; set; }
    public int DayIndex { get; set; }
    public int StartPeriodIndex { get; set; }
    public int Duration { get; set; }
    public ActivitySummaryDto? Activity { get; set; }
}

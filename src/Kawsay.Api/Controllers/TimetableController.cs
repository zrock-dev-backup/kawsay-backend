using Api.Data;
using Application.DTOs;
using Application.Models;
using Application.Services;
using Application.Core;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/timetables")]
public class TimetableController(
    TimetableService service,
    AcademicStructureService academicStructureService,
    TimetableGenerationService generationService) : ControllerBase // <-- Injected Generation Service
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TimetableStructure>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TimetableStructure>>> GetTimetables()
    {
        var timetables = await service.GetAllAsync();
        return Ok(timetables.Select(MapToDto));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TimetableStructure), StatusCodes.Status200OK)]
    public async Task<ActionResult<TimetableStructure>> CreateTimetable([FromBody] CreateTimetableRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Timetable name is required." });

        if (request.StartDate >= request.EndDate)
            return BadRequest(new { message = "Start date must be before end date." });

        if (request.Days.Count == 0)
            return BadRequest(new { message = "At least one day is required." });

        if (request.Periods.Count == 0)
            return BadRequest(new { message = "At least one period is required." });

        var timetable = new Timetable
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = request.Days.Select(dayName => new Day { Name = dayName }).ToList(),
            Periods = request.Periods.Select(period => new Period
            {
                Start = period.Start,
                End = period.End
            }).ToList()
        };

        var createdTimetable = await service.CreateTimetableAsync(timetable);
        var dto = MapToDto(createdTimetable);

        return CreatedAtAction(nameof(GetTimetable), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TimetableStructure), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimetableStructure>> GetTimetable(int id)
    {
        var timetable = await service.GetByIdAsync(id);
        if (timetable == null) return NotFound(new { message = $"Timetable with ID {id} not found." });
        return Ok(MapToDto(timetable));
    }

    /// <summary>
    /// THE IGNITION SWITCH: Calls the solver, generates the timetable, and saves it to StagedPlacements.
    /// </summary>
    [HttpPost("{id:int}/generate")]
    [ProducesResponseType(typeof(GeneratedTimetableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneratedTimetableDto>> GenerateTimetable(int id, CancellationToken cancellationToken)
    {
        // Fire the starter motor
        var result = await generationService.GenerateTimetableAsync(id, cancellationToken);
        
        if (result.IsFailure)
        {
            return result.Error.Type == ErrorType.NotFound 
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message }) 
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
        }

        // Timetable generated and staged in DB successfully.
        return Ok(result.Value);
    }

    private static TimetableStructure MapToDto(Timetable timetable)
    {
        return new TimetableStructure
        {
            Id = timetable.Id,
            Name = timetable.Name,
            StartDate = timetable.StartDate,
            EndDate = timetable.EndDate,
            Days = timetable.Days.Select(d => new TimetableDay
            {
                Id = d.Id,
                Name = d.Name
            }).ToList(),
            Periods = timetable.Periods.Select(p => new TimetablePeriod
            {
                Id = p.Id,
                Start = p.Start,
                End = p.End
            }).ToList()
        };
    }
}

using Api.Data;
using Application.DTOs;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/timetables")]
public class TimetableController(
    TimetableService service,
    AcademicStructureService academicStructureService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TimetableStructure>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TimetableStructure>>> GetTimetables()
    {
        var timetables = await service.GetAllAsync();
        return Ok(timetables.Select(MapToDto));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TimetableStructure), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpGet("master")]
    [ProducesResponseType(typeof(TimetableStructure), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimetableStructure>> GetMasterTimetable()
    {
        var master = await service.GetMasterTimetableAsync();
        return master == null
            ? NotFound(new { message = "No master timetable is currently configured." })
            : Ok(MapToDto(master));
    }

    [HttpPost("{id:int}/publish")]
    [ProducesResponseType(typeof(TimetableStructure), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimetableStructure>> PublishTimetable(int id)
    {
        var published = await service.PublishTimetableAsync(id);
        return published == null
            ? NotFound(new { message = $"Timetable with ID {id} not found." })
            : Ok(MapToDto(published));
    }

    [HttpGet("{timetableId:int}/cohorts")]
    [ProducesResponseType(typeof(IEnumerable<CohortDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CohortDetailDto>>> GetCohortsForTimetable(int timetableId)
    {
        var cohorts = await academicStructureService.GetCohortsByTimetableAsync(timetableId);
        return Ok(cohorts);
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

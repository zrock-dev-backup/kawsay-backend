using Api.Data;
using Application.DTOs;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/timetable")]
public class TimetableController(
    TimetableService service,
    AcademicStructureService academicStructureService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimetableStructure>>> GetTimetables()
    {
        var timetables = await service.GetAllAsync();
        return Ok(timetables.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<TimetableStructure>> CreateTimetable([FromBody] CreateTimetableRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Timetable name is required." });
        if (request.Days.Count == 0)
            return BadRequest(new { message = "At least one day is required." });
        if (request.Periods.Count == 0)
            return BadRequest(new { message = "At least one period is required." });

        var timetable = new Timetable
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Days = request.Days.Select(dayName => new Day
                {
                    Name = dayName
                })
                .ToList(),
            Periods = request.Periods.Select(period => new Period
                {
                    Start = period.Start,
                    End = period.End
                })
                .ToList()
        };
        var createdTimetable = await service.CreateTimetableAsync(timetable);

        var createdTimetableDto = MapToDto(createdTimetable);
        return CreatedAtAction(nameof(GetTimetable), new { id = createdTimetableDto.Id }, createdTimetableDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimetableStructure>> GetTimetable(int id)
    {
        var timetable = await service.GetByIdAsync(id);
        if (timetable == null) return NotFound();
        return Ok(MapToDto(timetable));
    }

    [HttpGet("master")]
    public async Task<ActionResult<TimetableStructure>> GetMasterTimetable()
    {
        var master = await service.GetMasterTimetableAsync();
        return master == null ? NotFound(new { message = "No timetables are available." }) : Ok(MapToDto(master));
    }

    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<TimetableStructure>> PublishTimetable(int id)
    {
        var published = await service.PublishTimetableAsync(id);
        return published == null ? NotFound() : Ok(MapToDto(published));
    }

    [HttpGet("{timetableId:int}/cohorts")]
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
                })
                .ToList(),
            Periods = timetable.Periods
                .Select(p => new TimetablePeriod
                {
                    Id = p.Id,
                    Start = p.Start,
                    End = p.End
                })
                .ToList()
        };
    }
}
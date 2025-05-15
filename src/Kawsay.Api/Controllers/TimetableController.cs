using Api.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class TimetableController(ITimetableRepository repository) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TimetableStructure>> CreateTimetable([FromBody] CreateTimetableRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Timetable name is required." });
        if (request.Days == null || request.Days.Count == 0)
            return BadRequest(new { message = "At least one day is required." });
        if (request.Periods == null || request.Periods.Count == 0)
            return BadRequest(new { message = "At least one period is required." });

        var timetableEntity = new TimetableEntity
        {
            Name = request.Name,
            Days = request.Days.Select(d => new TimetableDayEntity { Name = d }).ToList(),
            Periods = request.Periods.Select(p => new TimetablePeriodEntity
            {
                Start = p.Start,
                End = p.End
            }).ToList()
        };
        await repository.AddAsync(timetableEntity);
        var createdTimetableDto = new TimetableStructure
        {
            Id = timetableEntity.Id,
            Name = timetableEntity.Name,
            Days = timetableEntity.Days.Select(d => new TimetableDay
            {
                Id = d.Id,
                Name = d.Name
            }).ToList(),
            Periods = timetableEntity.Periods
                .Select(p => new TimetablePeriod
                {
                    Id = p.Id,
                    Start = p.Start,
                    End = p.End
                }).ToList()
        };
        return CreatedAtAction(nameof(GetTimetable), new { id = createdTimetableDto.Id }, createdTimetableDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimetableStructure>> GetTimetable(int id)
    {
        var timetable = await repository.GetByIdAsync(id);
        if (timetable == null) return NotFound();
        var timetableDto = new TimetableStructure
        {
            Id = timetable.Id,
            Name = timetable.Name,
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
                })
                .ToList()
        };

        return Ok(timetableDto);
    }

    [HttpGet]
    [Route("/kawsay/timetables")]
    public async Task<ActionResult<IEnumerable<TimetableStructure>>> GetTimetables()
    {
        var timetables = await repository.GetAllAsync();
        return Ok(timetables.Select(t => new TimetableStructure { Id = t.Id, Name = t.Name }));
    }
}
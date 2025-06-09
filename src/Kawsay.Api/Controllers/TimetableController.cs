using Api.Data;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class TimetableController(TimetableService service) : ControllerBase
{
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

        var createdTimetableDto = new TimetableStructure
        {
            Id = createdTimetable.Id,
            Name = createdTimetable.Name,
            StartDate = createdTimetable.StartDate,
            EndDate = createdTimetable.EndDate,
            Days = createdTimetable.Days.Select(d => new TimetableDay
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToList(),
            Periods = createdTimetable.Periods
                .Select(p => new TimetablePeriod
                {
                    Id = p.Id,
                    Start = p.Start,
                    End = p.End
                })
                .ToList()
        };
        return CreatedAtAction(nameof(GetTimetable), new { id = createdTimetableDto.Id }, createdTimetableDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimetableStructure>> GetTimetable(int id)
    {
        var timetable = await service.GetByIdAsync(id);
        if (timetable == null) return NotFound();
        var timetableDto = new TimetableStructure
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
        var timetables = await service.GetAllAsync();
        return Ok(timetables.Select(t => new TimetableStructure { Id = t.Id, Name = t.Name }));
    }
}
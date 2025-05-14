using Api.Data;
using Api.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class TimetableController : ControllerBase
{
    private readonly KawsayDbContext _context;

    public TimetableController(KawsayDbContext context)
    {
        _context = context;
    }

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
            Periods = request.Periods.Select(p => new TimetablePeriodEntity { Start = p.Start, End = p.End }).ToList()
        };
        _context.Timetables.Add(timetableEntity);
        await _context.SaveChangesAsync();
        var createdTimetableDto = new TimetableStructure
        {
            Id = timetableEntity.Id,
            Name = timetableEntity.Name,
            Days = timetableEntity.Days.Select(d => new TimetableDay { Id = d.Id, Name = d.Name }).ToList(),
            Periods = timetableEntity.Periods
                .Select(p => new TimetablePeriod { Id = p.Id, Start = p.Start, End = p.End }).ToList()
        };
        return CreatedAtAction(nameof(GetTimetable), new { id = createdTimetableDto.Id }, createdTimetableDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TimetableStructure>> GetTimetable(int id)
    {
        var timetable = await _context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (timetable == null) return NotFound();


        var timetableDto = new TimetableStructure
        {
            Id = timetable.Id,
            Name = timetable.Name,
            Days = timetable.Days.Select(d => new TimetableDay { Id = d.Id, Name = d.Name }).ToList(),
            Periods = timetable.Periods.Select(p => new TimetablePeriod { Id = p.Id, Start = p.Start, End = p.End })
                .ToList()
        };

        return Ok(timetableDto);
    }

    [HttpGet]
    [Route("/kawsay/timetables")]
    public async Task<ActionResult<IEnumerable<TimetableStructure>>> GetTimetables()
    {
        var timetables = await _context.Timetables
            .Select(t => new TimetableStructure { Id = t.Id, Name = t.Name })
            .ToListAsync();
        return Ok(timetables);
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<TimetableStructure>> UpdateTimetable(int id,
        [FromBody] UpdateTimetableRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID in URL and body must match." });
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Timetable name is required." });
        if (request.Days == null || request.Days.Count == 0)
            return BadRequest(new { message = "At least one day is required." });
        if (request.Periods == null || request.Periods.Count == 0)
            return BadRequest(new { message = "At least one period is required." });

        var timetableEntity = await _context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (timetableEntity == null) return NotFound();
        timetableEntity.Name = request.Name;
        _context.TimetableDays.RemoveRange(timetableEntity.Days);
        _context.TimetablePeriods.RemoveRange(timetableEntity.Periods);
        timetableEntity.Days = request.Days.Select(d => new TimetableDayEntity { Name = d }).ToList();
        timetableEntity.Periods =
            request.Periods.Select(p => new TimetablePeriodEntity { Start = p.Start, End = p.End }).ToList();
        await _context.SaveChangesAsync();

        var updatedTimetableDto = new TimetableStructure
        {
            Id = timetableEntity.Id,
            Name = timetableEntity.Name,
            Days = timetableEntity.Days.Select(d => new TimetableDay { Id = d.Id, Name = d.Name }).ToList(),
            Periods = timetableEntity.Periods
                .Select(p => new TimetablePeriod { Id = p.Id, Start = p.Start, End = p.End }).ToList()
        };

        return Ok(updatedTimetableDto);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTimetable(int id)
    {
        var timetable = await _context.Timetables.FindAsync(id);
        if (timetable == null) return NotFound();
        _context.Timetables.Remove(timetable);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
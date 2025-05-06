using KawsayApiMockup.Data;
using KawsayApiMockup.DTOs;
using KawsayApiMockup.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KawsayApiMockup.Controllers;

[ApiController]
[Route("kawsay")]
public class ClassesController : ControllerBase
{
    private readonly KawsayDbContext _context;

    public ClassesController(KawsayDbContext context)
    {
        _context = context;
    }


    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<Class>>> GetClassesByTimetable([FromQuery] int timetableId)
    {
        var timetableExists = await _context.Timetables.AnyAsync(t => t.Id == timetableId);
        if (!timetableExists) return NotFound(new { message = $"Timetable with ID {timetableId} not found." });


        var classes = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Occurrences)
            .Where(c => c.TimetableId == timetableId)
            .ToListAsync();


        var classDtos = classes.Select(cls => new Class
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            Course = new Course { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
            Teacher = cls.Teacher != null
                ? new Teacher { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type }
                : null,
            Occurrences = cls.Occurrences.Select(o => new ClassOccurrence
            {
                Id = o.Id,
                DayId = o.DayId,
                StartPeriodId = o.StartPeriodId,
                Length = o.Length
            }).ToList()
        }).ToList();

        return Ok(classDtos);
    }

    [HttpGet("class/{id}")]
    public async Task<ActionResult<Class>> GetClass(int id)
    {
        var cls = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Occurrences)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (cls == null) return NotFound();


        var classDto = new Class
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            Course = new Course { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
            Teacher = cls.Teacher != null
                ? new Teacher { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type }
                : null,
            Occurrences = cls.Occurrences.Select(o => new ClassOccurrence
            {
                Id = o.Id,
                DayId = o.DayId,
                StartPeriodId = o.StartPeriodId,
                Length = o.Length
            }).ToList()
        };

        return Ok(classDto);
    }

    [HttpPost("class")]
    public async Task<ActionResult<Class>> CreateClass([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.Occurrences == null || request.Occurrences.Count == 0)
            return BadRequest(new { message = "At least one occurrence is required." });
        if (request.Occurrences.Any(o => o.Length <= 0))
            return BadRequest(new { message = "Occurrence length must be positive." });


        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null) return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });

        TeacherEntity? teacher = null;
        if (request.TeacherId.HasValue)
        {
            teacher = await _context.Teachers.FindAsync(request.TeacherId.Value);
            if (teacher == null)
                return BadRequest(new { message = $"Teacher with ID {request.TeacherId.Value} not found." });
        }

        var timetable = await _context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Where(t => t.Id == request.TimetableId)
            .FirstOrDefaultAsync();
        if (timetable == null)
            return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });


        foreach (var occ in request.Occurrences)
        {
            if (!timetable.Days.Any(d => d.Id == occ.DayId))
                return BadRequest(new
                    { message = $"Day ID {occ.DayId} not found in timetable {request.TimetableId}." });
            if (!timetable.Periods.Any(p => p.Id == occ.StartPeriodId))
                return BadRequest(new
                    { message = $"Period ID {occ.StartPeriodId} not found in timetable {request.TimetableId}." });
            var startPeriodIndex = timetable.Periods.OrderBy(p => p.Start).ToList()
                .FindIndex(p => p.Id == occ.StartPeriodId);
            if (startPeriodIndex != -1 && startPeriodIndex + occ.Length > timetable.Periods.Count)
                return BadRequest(new
                {
                    message =
                        $"Occurrence length {occ.Length} for period ID {occ.StartPeriodId} exceeds available periods in timetable {request.TimetableId}."
                });
        }


        var classEntity = new ClassEntity
        {
            TimetableId = request.TimetableId,
            CourseId = request.CourseId,
            TeacherId = request.TeacherId,
            Occurrences = request.Occurrences.Select(o => new ClassOccurrenceEntity
            {
                DayId = o.DayId,
                StartPeriodId = o.StartPeriodId,
                Length = o.Length
            }).ToList()
        };


        _context.Classes.Add(classEntity);
        await _context.SaveChangesAsync();


        var createdClassEntity = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Occurrences)
            .Where(c => c.Id == classEntity.Id)
            .FirstOrDefaultAsync();


        var createdClassDto = new Class
        {
            Id = createdClassEntity!.Id,
            TimetableId = createdClassEntity.TimetableId,
            Course = new Course
            {
                Id = createdClassEntity.Course.Id, Name = createdClassEntity.Course.Name,
                Code = createdClassEntity.Course.Code
            },
            Teacher = createdClassEntity.Teacher != null
                ? new Teacher
                {
                    Id = createdClassEntity.Teacher.Id, Name = createdClassEntity.Teacher.Name,
                    Type = createdClassEntity.Teacher.Type
                }
                : null,
            Occurrences = createdClassEntity.Occurrences.Select(o => new ClassOccurrence
                { Id = o.Id, DayId = o.DayId, StartPeriodId = o.StartPeriodId, Length = o.Length }).ToList()
        };


        return CreatedAtAction(nameof(GetClass), new { id = createdClassDto.Id }, createdClassDto);
    }


    [HttpPut("class/{id}")]
    public async Task<ActionResult<Class>> UpdateClass(int id, [FromBody] UpdateClassRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID in URL and body must match." });
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.Occurrences == null || request.Occurrences.Count == 0)
            return BadRequest(new { message = "At least one occurrence is required." });
        if (request.Occurrences.Any(o => o.Length <= 0))
            return BadRequest(new { message = "Occurrence length must be positive." });


        var existingClass = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Occurrences)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (existingClass == null) return NotFound();


        if (existingClass.TimetableId != request.TimetableId)
            return BadRequest(new
                { message = $"Cannot change timetableId ({request.TimetableId}) for existing class ID {id}." });


        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null)
            return BadRequest(new { message = $"Course with ID {request.CourseId} not found during update." });

        TeacherEntity? teacher = null;
        if (request.TeacherId.HasValue)
        {
            teacher = await _context.Teachers.FindAsync(request.TeacherId.Value);
            if (teacher == null)
                return BadRequest(new
                    { message = $"Teacher with ID {request.TeacherId.Value} not found during update." });
        }


        var timetable = await _context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Where(t => t.Id == request.TimetableId)
            .FirstOrDefaultAsync();
        if (timetable == null)
            return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });


        foreach (var occ in request.Occurrences)
        {
            if (!timetable.Days.Any(d => d.Id == occ.DayId))
                return BadRequest(new
                    { message = $"Day ID {occ.DayId} not found in timetable {request.TimetableId}." });
            if (!timetable.Periods.Any(p => p.Id == occ.StartPeriodId))
                return BadRequest(new
                    { message = $"Period ID {occ.StartPeriodId} not found in timetable {request.TimetableId}." });
            var startPeriodIndex = timetable.Periods.OrderBy(p => p.Start).ToList()
                .FindIndex(p => p.Id == occ.StartPeriodId);
            if (startPeriodIndex != -1 && startPeriodIndex + occ.Length > timetable.Periods.Count)
                return BadRequest(new
                {
                    message =
                        $"Occurrence length {occ.Length} for period ID {occ.StartPeriodId} exceeds available periods in timetable {request.TimetableId}."
                });
        }


        existingClass.CourseId = request.CourseId;
        existingClass.TeacherId = request.TeacherId;


        var existingOccurrences = existingClass.Occurrences.ToList();
        var requestedOccurrences = request.Occurrences.ToList();


        foreach (var existingOcc in existingOccurrences)
            if (!requestedOccurrences.Any(reqOcc => reqOcc.Id == existingOcc.Id && reqOcc.Id != 0))
                _context.ClassOccurrences.Remove(existingOcc);


        foreach (var requestedOcc in requestedOccurrences)
            if (requestedOcc.Id == 0)
            {
                existingClass.Occurrences.Add(new ClassOccurrenceEntity
                {
                    DayId = requestedOcc.DayId,
                    StartPeriodId = requestedOcc.StartPeriodId,
                    Length = requestedOcc.Length
                });
            }
            else
            {
                var existingOcc = existingOccurrences.FirstOrDefault(eo => eo.Id == requestedOcc.Id);
                if (existingOcc != null)
                {
                    existingOcc.DayId = requestedOcc.DayId;
                    existingOcc.StartPeriodId = requestedOcc.StartPeriodId;
                    existingOcc.Length = requestedOcc.Length;
                    _context.ClassOccurrences.Update(existingOcc);
                }
                else
                {
                    return BadRequest(new
                        { message = $"Occurrence with ID {requestedOcc.Id} not found for class {id}." });
                }
            }


        await _context.SaveChangesAsync();


        var updatedClassEntity = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Occurrences)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();


        var updatedClassDto = new Class
        {
            Id = updatedClassEntity!.Id,
            TimetableId = updatedClassEntity.TimetableId,
            Course = new Course
            {
                Id = updatedClassEntity.Course.Id, Name = updatedClassEntity.Course.Name,
                Code = updatedClassEntity.Course.Code
            },
            Teacher = updatedClassEntity.Teacher != null
                ? new Teacher
                {
                    Id = updatedClassEntity.Teacher.Id, Name = updatedClassEntity.Teacher.Name,
                    Type = updatedClassEntity.Teacher.Type
                }
                : null,
            Occurrences = updatedClassEntity.Occurrences.Select(o => new ClassOccurrence
                { Id = o.Id, DayId = o.DayId, StartPeriodId = o.StartPeriodId, Length = o.Length }).ToList()
        };


        return Ok(updatedClassDto);
    }


    [HttpDelete("class/{id}")]
    public async Task<IActionResult> DeleteClass(int id)
    {
        var cls = await _context.Classes.FindAsync(id);
        if (cls == null) return NotFound();

        _context.Classes.Remove(cls);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
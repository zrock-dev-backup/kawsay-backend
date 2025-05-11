using kawsay.Data;
using kawsay.DTOs;
using kawsay.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kawsay.Controllers;

[ApiController]
[Route("kawsay")]
public class ClassesController(KawsayDbContext context) : ControllerBase
{
    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<Class>>> GetClassesByTimetable([FromQuery] int timetableId)
    {
        var timetableExists = await context.Timetables.AnyAsync(t => t.Id == timetableId);
        if (!timetableExists) return NotFound(new { message = $"Timetable with ID {timetableId} not found." });

        var classes = await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.PeriodPreferences)
            .Where(c => c.TimetableId == timetableId)
            .ToListAsync();

        var classDtos = classes.Select(cls => new Class
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            Course = new Course { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
            Teacher = new Teacher { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type },
            PeriodPreferencesList = cls.PeriodPreferences.Select(o => new PeriodPreferencesDto()
            {
                StartPeriodId = o.StartPeriodId,
            }).ToList()
        }).ToList();

        return Ok(classDtos);
    }

    [HttpGet("class/{id}")]
    public async Task<ActionResult<Class>> GetClass(int id)
    {
        var cls = await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.PeriodPreferences)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (cls == null) return NotFound();


        var classDto = new Class
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            Course = new Course { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
            Teacher = new Teacher { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type },
            PeriodPreferencesList = cls.PeriodPreferences.Select(o => new PeriodPreferencesDto
            {
                StartPeriodId = o.StartPeriodId,
            }).ToList()
        };

        return Ok(classDto);
    }

    [HttpPost("class")]
    public async Task<ActionResult<Class>> CreateClass([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errorResponse = new
            {
                message = "Invalid request",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .ToList()
            };
            return BadRequest(errorResponse);
        }

        if (request.PeriodPreferencesList.Count == 0 || request.Frequency == 0 || request.Length == 0)
            return BadRequest(new { message = "A field has a 0 value" });

        var course = await context.Courses.FindAsync(request.CourseId);
        if (course == null) return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });

        var teacher = await context.Teachers.FindAsync(request.TeacherId);
        if (teacher == null)
            return BadRequest(new { message = $"Teacher with ID {request.TeacherId} not found." });

        var timetable = await context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Where(t => t.Id == request.TimetableId)
            .FirstOrDefaultAsync();
        if (timetable == null)
            return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });

        if (request.Length > timetable.Periods.Count)
        {
            return BadRequest(new
            {
                message =
                    $"Class length {request.Length} for class exceeds available periods in timetable {request.TimetableId}."
            });
        }

        foreach (var periodPreference in request.PeriodPreferencesList)
        {
            if (timetable.Periods.All(p => p.Id != periodPreference.StartPeriodId))
                return BadRequest(new
                {
                    message =
                        $"Period ID {periodPreference.StartPeriodId} not found in timetable {request.TimetableId}."
                });
        }

        var classEntity = new ClassEntity
        {
            TimetableId = request.TimetableId,
            CourseId = request.CourseId,
            TeacherId = request.TeacherId,
            Frequency = request.Frequency,
            Length = request.Length,
            PeriodPreferences = request.PeriodPreferencesList.Select(o => new PeriodPreferenceEntity()
            {
                StartPeriodId = o.StartPeriodId,
            }).ToList()
        };
        context.Classes.Add(classEntity);
        await context.SaveChangesAsync();

        var createdClassEntity = await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.PeriodPreferences)
            .Where(c => c.Id == classEntity.Id)
            .FirstOrDefaultAsync();

        var createdClassDto = new Class
        {
            Id = createdClassEntity!.Id,
            TimetableId = createdClassEntity.TimetableId,
            Length = createdClassEntity.Length,
            Frequency = createdClassEntity.Frequency,
            Course = new Course
            {
                Id = createdClassEntity.Course.Id,
                Name = createdClassEntity.Course.Name,
                Code = createdClassEntity.Course.Code
            },
            Teacher = new Teacher
            {
                Id = createdClassEntity.Teacher.Id,
                Name = createdClassEntity.Teacher.Name,
                Type = createdClassEntity.Teacher.Type
            },
            PeriodPreferencesList = createdClassEntity.PeriodPreferences.Select(o => new PeriodPreferencesDto
                { StartPeriodId = o.StartPeriodId }).ToList()
        };

        return CreatedAtAction(nameof(GetClass), new { id = createdClassDto.Id }, createdClassDto);
    }
}
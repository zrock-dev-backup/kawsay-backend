using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay")]
public class ClassesController(
    ClassService classService,
    TeacherService teacherService,
    TimetableService timetableService,
    CourseService courseService) : ControllerBase
{
    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<ClassDto>>> GetClassesByTimetable([FromQuery] int timetableId)
    {
        var timetable = await timetableService.GetByIdAsync(timetableId);
        if (timetable == null) return NotFound(new { message = $"Timetable with ID {timetableId} not found." });

        var classes = await classService.GetAllAsync(timetableId);
        var classDtos = classes.Select(lecture => new ClassDto
        {
            Id = lecture.Id,
            TimetableId = lecture.TimetableId,
            Frequency = lecture.Frequency,
            Length = lecture.Length,
            CourseDto = new CourseDto
            {
                Id = lecture.CourseDto.Id,
                Name = lecture.CourseDto.Name,
                Code = lecture.CourseDto.Code
            },
            TeacherDto = new TeacherDto
            {
                Id = lecture.TeacherDto.Id,
                Name = lecture.TeacherDto.Name,
                Type = lecture.TeacherDto.Type
            },
            ClassOccurrences = lecture.ClassOccurrences.Select(o => new ClassOccurrenceDto
            {
                StartPeriodId = o.StartPeriodId,
                DayId = o.DayId,
            }).ToList()
        }).ToList();

        return Ok(classDtos);
    }

    [HttpGet("class/{id:int}")]
    public async Task<ActionResult<ClassDto>> GetClass(int id)
    {
        var cls = await classService.GetByIdAsync(id);
        if (cls == null) return NotFound();
        var classDto = new ClassDto
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            CourseDto = new CourseDto
            {
                Id = cls.CourseDto.Id,
                Name = cls.CourseDto.Name,
                Code = cls.CourseDto.Code
            },
            TeacherDto = new TeacherDto
            {
                Id = cls.TeacherDto.Id,
                Name = cls.TeacherDto.Name,
                Type = cls.TeacherDto.Type
            },
            ClassOccurrences = cls.ClassOccurrences.Select(o => new ClassOccurrenceDto
            {
                DayId = o.DayId,
                StartPeriodId = o.StartPeriodId,
            }).ToList()
        };

        return Ok(classDto);
    }

    [HttpPost("class")]
    public async Task<ActionResult<ClassDto>> CreateClass([FromBody] CreateClassRequest request)
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

        var course = await courseService.GetCourseByIdAsync(request.CourseId);
        if (course == null) return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });

        var teacher = await teacherService.GetByIdAsync(request.TeacherId);
        if (teacher == null)
            return BadRequest(new { message = $"Teacher with ID {request.TeacherId} not found." });

        var timetable = await timetableService.GetByIdAsync(request.TimetableId);
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

        foreach (var periodPreference in
                 request.PeriodPreferencesList.Where(periodPreference =>
                     timetable.Periods.All(p => p.Id != periodPreference.StartPeriodId)))
        {
            return BadRequest(new
            {
                message =
                    $"Period ID {periodPreference.StartPeriodId} not found in timetable {request.TimetableId}."
            });
        }


        var createdClassEntity = await classService.CreateClassAsync(request);

        var createdClassDto = new ClassDto
        {
            Id = createdClassEntity.Id,
            TimetableId = createdClassEntity.TimetableId,
            Length = createdClassEntity.Length,
            Frequency = createdClassEntity.Frequency,
            CourseDto = new CourseDto
            {
                Id = createdClassEntity.CourseDto.Id,
                Name = createdClassEntity.CourseDto.Name,
                Code = createdClassEntity.CourseDto.Code
            },
            TeacherDto = new TeacherDto
            {
                Id = createdClassEntity.TeacherDto.Id,
                Name = createdClassEntity.TeacherDto.Name,
                Type = createdClassEntity.TeacherDto.Type
            },
            ClassOccurrences = createdClassEntity.ClassOccurrences.Select(o => new ClassOccurrenceDto()
            {
                StartPeriodId = o.StartPeriodId,
                DayId = o.DayId,
            }).ToList()
        };

        return CreatedAtAction(nameof(GetClass), new { id = createdClassDto.Id }, createdClassDto);
    }
}
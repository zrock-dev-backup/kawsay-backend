using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay")]
public class ClassesController(
    IClassRepository classRepository,
    ITeacherRepository teacherRepository,
    ITimetableRepository timetableRepository,
    ICourseRepository courseRepository) : ControllerBase
{
    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<ClassDto>>> GetClassesByTimetable([FromQuery] int timetableId)
    {
        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null) return NotFound(new { message = $"Timetable with ID {timetableId} not found." });

        var classes = await classRepository.GetAllAsync();

        var classDtos = classes.Select(cls => new ClassDto
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            Frequency = cls.Frequency,
            Length = cls.Length,
            CourseDto = new CourseDto { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
            TeacherDto = new TeacherDto { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type },
            ClassOccurrences = cls.ClassOccurrences.Select(o => new ClassOccurrenceDto
            {
                StartPeriodId = o.StartPeriodId,
                DayId = o.DayId,
            }).ToList()
        }).ToList();

        return Ok(classDtos);
    }

    [HttpGet("class/{id}")]
    public async Task<ActionResult<ClassDto>> GetClass(int id)
    {
        var cls = await classRepository.GetByIdAsync(id);
        if (cls == null) return NotFound();
        var classDto = new ClassDto
        {
            Id = cls.Id,
            TimetableId = cls.TimetableId,
            CourseDto = new CourseDto
            {
                Id = cls.Course.Id,
                Name = cls.Course.Name,
                Code = cls.Course.Code
            },
            TeacherDto = new TeacherDto
            {
                Id = cls.Teacher.Id,
                Name = cls.Teacher.Name,
                Type = cls.Teacher.Type
            },
            ClassOccurrences = cls.ClassOccurrences.Select(o => new ClassOccurrenceDto()
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

        var course = await courseRepository.GetByIdAsync(request.CourseId);
        if (course == null) return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });

        var teacher = await teacherRepository.GetByIdAsync(request.TeacherId);
        if (teacher == null)
            return BadRequest(new { message = $"Teacher with ID {request.TeacherId} not found." });

        var timetable = await timetableRepository.GetByIdAsync(request.TimetableId);
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
            PeriodPreferences = request.PeriodPreferencesList.Select(o => new PeriodPreferenceEntity
            {
                StartPeriodId = o.StartPeriodId,
            }).ToList()
        };

        await classRepository.AddAsync(classEntity);
        var createdClassEntity = await classRepository.GetByIdAsync(classEntity.Id);

        if (createdClassEntity == null)
        {
            return BadRequest(new { message = "Class creation failed." });
        }

        var createdClassDto = new ClassDto
        {
            Id = createdClassEntity.Id,
            TimetableId = createdClassEntity.TimetableId,
            Length = createdClassEntity.Length,
            Frequency = createdClassEntity.Frequency,
            CourseDto = new CourseDto
            {
                Id = createdClassEntity.Course.Id,
                Name = createdClassEntity.Course.Name,
                Code = createdClassEntity.Course.Code
            },
            TeacherDto = new TeacherDto
            {
                Id = createdClassEntity.Teacher.Id,
                Name = createdClassEntity.Teacher.Name,
                Type = createdClassEntity.Teacher.Type
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
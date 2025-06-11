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
    CourseService courseService,
    AcademicStructureService structureService) : ControllerBase
{
    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<ClassDto>>> GetClassesByTimetable([FromQuery] int timetableId)
    {
        var classes = await classService.GetAllAsync(timetableId);
        var classDtos = classes.Select(c => new ClassDto
        {
            Id = c.Id,
            TimetableId = c.TimetableId,
            Frequency = c.Frequency,
            Length = c.Length,
            ClassType = c.ClassType,
            CourseDto = c.CourseDto,
            TeacherDto = c.TeacherDto,
            ClassOccurrences = c.ClassOccurrences,
            PeriodPreferences = c.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId
            }).ToList()
        }).ToList();

        return Ok(classDtos);
    }

    [HttpGet("class/{id:int}")]
    public async Task<ActionResult<ClassDto>> GetClass(int id)
    {
        var c = await classService.GetByIdAsync(id);
        if (c == null) return NotFound();

        var classDto = new ClassDto
        {
            Id = c.Id,
            TimetableId = c.TimetableId,
            CourseDto = c.CourseDto,
            TeacherDto = c.TeacherDto,
            Length = c.Length,
            Frequency = c.Frequency,
            ClassType = c.ClassType,
            ClassOccurrences = c.ClassOccurrences,
            PeriodPreferences = c.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId
            }).ToList()
        };
        return Ok(classDto);
    }

    [HttpPost("class")]
    public async Task<ActionResult<ClassDto>> CreateClass([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Invalid model state.",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        var timetable = await timetableService.GetByIdAsync(request.TimetableId);
        if (timetable == null)
            return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });

        if (await courseService.GetCourseByIdAsync(request.CourseId) == null)
            return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });

        if (request.TeacherId.HasValue && await teacherService.GetByIdAsync(request.TeacherId.Value) == null)
            return BadRequest(new { message = $"Teacher with ID {request.TeacherId.Value} not found." });

        // Hierarchy links validation
        switch (request.ClassType)
        {
            case ClassTypeDto.Masterclass:
                if (!request.StudentGroupId.HasValue)
                    return BadRequest(new { message = "StudentGroupId is required for Masterclasses." });
                if (request.SectionId.HasValue)
                    return BadRequest(new { message = "SectionId must be null for Masterclasses." });
                if (await structureService.GetStudentGroupByIdAsync(request.StudentGroupId.Value) == null)
                    return BadRequest(new
                        { message = $"StudentGroup with ID {request.StudentGroupId.Value} not found." });
                break;
            case ClassTypeDto.Lab:
                if (!request.SectionId.HasValue)
                    return BadRequest(new { message = "SectionId is required for Labs." });
                if (request.StudentGroupId.HasValue)
                    return BadRequest(new { message = "StudentGroupId must be null for Labs." });
                if (await structureService.GetSectionWithStudentsAsync(request.SectionId.Value) == null)
                    return BadRequest(new { message = $"Section with ID {request.SectionId.Value} not found." });
                break;
        }

        var createdClassModel = await classService.CreateClassAsync(request);

        var createdClassDto = new ClassDto
        {
            Id = createdClassModel.Id,
            TimetableId = createdClassModel.TimetableId,
            Length = createdClassModel.Length,
            Frequency = createdClassModel.Frequency,
            ClassType = createdClassModel.ClassType,
            CourseDto = createdClassModel.CourseDto,
            TeacherDto = createdClassModel.TeacherDto,
            ClassOccurrences = createdClassModel.ClassOccurrences,
            PeriodPreferences = createdClassModel.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId,
            }).ToList(),
        };

        return CreatedAtAction(nameof(GetClass), new { id = createdClassDto.Id }, createdClassDto);
    }
}
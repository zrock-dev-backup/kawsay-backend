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
        var classes = await classService.GetAllAsync(timetableId);
        var response = classes.Select(lectureModel => new ClassDto
        {
            Id = lectureModel.Id,
            StartDate = lectureModel.StartDate,
            EndDate = lectureModel.EndDate,
            TimetableId = lectureModel.TimetableId,
            Frequency = lectureModel.Frequency,
            Length = lectureModel.Length,
            ClassType = lectureModel.ClassType,
            CourseId = lectureModel.CourseDto.Id,
            CourseName = lectureModel.CourseDto.Name,
            CourseCode = lectureModel.CourseDto.Code,
            TeacherId = lectureModel.TeacherDto?.Id,
            TeacherName = lectureModel.TeacherDto?.Name,
            ClassOccurrences = lectureModel.ClassOccurrences,
            PeriodPreferences = lectureModel.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId
            }).ToList()
        }).ToList();

        return Ok(response);
    }

    [HttpGet("class/{id:int}")]
    public async Task<ActionResult<ClassDto>> GetClass(int id)
    {
        var createdClassModel = await classService.GetByIdAsync(id);
        if (createdClassModel == null) return NotFound();

        var responseDto = new ClassDto
        {
            Id = createdClassModel.Id,
            StartDate = createdClassModel.StartDate,
            EndDate = createdClassModel.EndDate,
            TimetableId = createdClassModel.TimetableId,
            CourseId = createdClassModel.CourseDto.Id,
            CourseName = createdClassModel.CourseDto.Name,
            CourseCode = createdClassModel.CourseDto.Code,
            TeacherId = createdClassModel.TeacherDto?.Id,
            TeacherName = createdClassModel.TeacherDto?.Name,
            Length = createdClassModel.Length,
            Frequency = createdClassModel.Frequency,
            Capacity = createdClassModel.Capacity,
            ClassType = createdClassModel.ClassType,
            ClassOccurrences = createdClassModel.ClassOccurrences,
            PeriodPreferences = createdClassModel.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId
            }).ToList()
        };
        return Ok(responseDto);
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

        var createdClassModel = await classService.CreateClassAsync(request);
        var responseDto = new ClassDto
        {
            Id = createdClassModel.Id,
            StartDate = createdClassModel.StartDate,
            EndDate = createdClassModel.EndDate,
            TimetableId = createdClassModel.TimetableId,
            Length = createdClassModel.Length,
            Frequency = createdClassModel.Frequency,
            ClassType = createdClassModel.ClassType,
            CourseId = createdClassModel.CourseDto.Id,
            CourseName = createdClassModel.CourseDto.Name,
            CourseCode = createdClassModel.CourseDto.Code,
            TeacherId = createdClassModel.TeacherDto?.Id,
            TeacherName = createdClassModel.TeacherDto?.Name,
            ClassOccurrences = createdClassModel.ClassOccurrences,
            PeriodPreferences = createdClassModel.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId,
            }).ToList(),
        };

        return CreatedAtAction(nameof(GetClass), new { id = createdClassModel.Id }, responseDto);
    }

    [HttpPut("class/{id:int}")]
    public async Task<IActionResult> UpdateClass(int id, [FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedClassModel = await classService.UpdateClassAsync(id, request);
            if (updatedClassModel == null)
            {
                return NotFound(new { message = $"Class with ID {id} not found." });
            }

            var responseDto = new ClassDto
            {
                Id = updatedClassModel.Id,
                StartDate = updatedClassModel.StartDate,
                EndDate = updatedClassModel.EndDate,
                TimetableId = updatedClassModel.TimetableId,
                CourseId = updatedClassModel.CourseDto.Id,
                CourseName = updatedClassModel.CourseDto.Name,
                CourseCode = updatedClassModel.CourseDto.Code,
                TeacherId = updatedClassModel.TeacherDto?.Id,
                TeacherName = updatedClassModel.TeacherDto?.Name,
                Length = updatedClassModel.Length,
                Frequency = updatedClassModel.Frequency,
                ClassType = updatedClassModel.ClassType,
                ClassOccurrences = updatedClassModel.ClassOccurrences,
                PeriodPreferences = updatedClassModel.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
                {
                    DayId = p.DayId,
                    StartPeriodId = p.StartPeriodId
                }).ToList()
            };
            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("class/{id:int}")]
    public async Task<IActionResult> DeleteClass(int id)
    {
        try
        {
            var success = await classService.DeleteClassAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Class with ID {id} not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.Out.WriteLine(ex);
            return StatusCode(500, new { message = "An internal error occurred." });
        }
    }
}
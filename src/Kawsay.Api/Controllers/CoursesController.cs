using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class CoursesController(CourseService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await service.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        var course = await service.GetCourseByIdAsync(id);
        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpGet("{courseId:int}/qualified-teachers")]
    public async Task<ActionResult<IEnumerable<TeacherDto>>> GetQualifiedTeachers(int courseId)
    {
        var teachers = await service.GetQualifiedTeachersForCourseAsync(courseId);
        return Ok(teachers);
    }
}
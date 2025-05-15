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

    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CourseDto createCourseRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var createdCourseDto = await service.CreateCourseAsync(createCourseRequest);
        return CreatedAtAction(nameof(GetCourse),
            new
            {
                id = createdCourseDto.Id
            },
            createdCourseDto);
    }
}
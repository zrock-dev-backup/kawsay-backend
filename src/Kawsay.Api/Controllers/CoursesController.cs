using Api.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class CoursesController(ICourseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await repository.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await repository.GetByIdAsync(id);
        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpPost]
    public async Task<ActionResult<Course>> CreateCourse([FromBody] Course courseDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var courseEntity = new CourseEntity { Name = courseDto.Name, Code = courseDto.Code };
        await repository.AddAsync(courseEntity);
        var createdCourseDto = new Course { Id = courseEntity.Id, Name = courseEntity.Name, Code = courseEntity.Code };
        return CreatedAtAction(nameof(GetCourse), new { id = createdCourseDto.Id }, createdCourseDto);
    }
}
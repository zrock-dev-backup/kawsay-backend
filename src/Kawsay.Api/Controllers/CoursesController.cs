using Api.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly KawsayDbContext _context;

    public CoursesController(KawsayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await _context.Courses
            .Select(c => new Course { Id = c.Id, Name = c.Name, Code = c.Code })
            .ToListAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await _context.Courses
            .Where(c => c.Id == id)
            .Select(c => new Course { Id = c.Id, Name = c.Name, Code = c.Code })
            .FirstOrDefaultAsync();
        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpPost]
    public async Task<ActionResult<Course>> CreateCourse([FromBody] Course courseDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var courseEntity = new CourseEntity { Name = courseDto.Name, Code = courseDto.Code };
        _context.Courses.Add(courseEntity);
        await _context.SaveChangesAsync();
        var createdCourseDto = new Course { Id = courseEntity.Id, Name = courseEntity.Name, Code = courseEntity.Code };
        return CreatedAtAction(nameof(GetCourse), new { id = createdCourseDto.Id }, createdCourseDto);
    }
}
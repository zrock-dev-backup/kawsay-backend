using kawsay.Data;
using kawsay.DTOs;
using kawsay.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kawsay.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly KawsayDbContext _context;

    public TeachersController(KawsayDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
    {
        var teachers = await _context.Teachers
            .Select(t => new Teacher { Id = t.Id, Name = t.Name, Type = t.Type })
            .ToListAsync();
        return Ok(teachers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Teacher>> GetTeacher(int id)
    {
        var teacher = await _context.Teachers
            .Where(t => t.Id == id)
            .Select(t => new Teacher { Id = t.Id, Name = t.Name, Type = t.Type })
            .FirstOrDefaultAsync();
        if (teacher == null) return NotFound();
        return Ok(teacher);
    }

    [HttpPost]
    public async Task<ActionResult<Teacher>> CreateTeacher([FromBody] Teacher teacherDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (teacherDto.Type != "Professor" && teacherDto.Type != "Faculty Practitioner")
            return BadRequest(new { message = "Invalid teacher type. Must be 'Professor' or 'Faculty Practitioner'." });


        var teacherEntity = new TeacherEntity { Name = teacherDto.Name, Type = teacherDto.Type };


        _context.Teachers.Add(teacherEntity);
        await _context.SaveChangesAsync();


        var createdTeacherDto = new Teacher
            { Id = teacherEntity.Id, Name = teacherEntity.Name, Type = teacherEntity.Type };

        return CreatedAtAction(nameof(GetTeacher), new { id = createdTeacherDto.Id }, createdTeacherDto);
    }
}
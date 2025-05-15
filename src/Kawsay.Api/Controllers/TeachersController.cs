using Api.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class TeachersController(ITeacherRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
    {
        return Ok(await repository.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Teacher>> GetTeacher(int id)
    {
        var teacher = await repository.GetByIdAsync(id);
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
        await repository.AddAsync(teacherEntity);

        var createdTeacherDto = new Teacher
            { Id = teacherEntity.Id, Name = teacherEntity.Name, Type = teacherEntity.Type };
        return CreatedAtAction(nameof(GetTeacher), new { id = createdTeacherDto.Id }, createdTeacherDto);
    }
}
#if DEBUG || STAGING
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class StudentsController(IStudentRepository studentRepository) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] StudentDto studentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var studentEntity = new StudentEntity
        {
            Name = studentDto.Name,
            Standing = Enum.TryParse<AcademicStanding>(studentDto.Standing, true, out var standing) 
                ? standing 
                : AcademicStanding.GoodStanding
        };

        var createdStudent = await studentRepository.AddAsync(studentEntity);

        var responseDto = new StudentDto
        {
            Id = createdStudent.Id,
            Name = createdStudent.Name,
            Standing = createdStudent.Standing.ToString()
        };

        return Created($"/kawsay/Students/{responseDto.Id}", responseDto);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        var student = await studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        var dto = new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Standing = student.Standing.ToString()
        };
        return Ok(dto);
    }
}
#endif

using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class TeachersController(TeacherService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeacherDto>>> GetTeachers()
    {
        return Ok(await service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeacherDto>> GetTeacher(int id)
    {
        var teacher = await service.GetByIdAsync(id);
        if (teacher == null) return NotFound();
        return Ok(teacher);
    }
}

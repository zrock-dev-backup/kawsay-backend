// Controllers/TeachersController.cs
using KawsayApiMockup.Data;
using KawsayApiMockup.DTOs;
using KawsayApiMockup.Entities; // Import Entities
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Import EF Core namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Use async methods

namespace KawsayApiMockup.Controllers
{
    [ApiController]
    [Route("kawsay/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly KawsayDbContext _context; // Inject DbContext

        public TeachersController(KawsayDbContext context)
        {
            _context = context;
        }

        [HttpGet] // GET /kawsay/teachers
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
        {
            // Fetch entities and map to DTOs
            var teachers = await _context.Teachers
                                         .Select(t => new Teacher { Id = t.Id, Name = t.Name, Type = t.Type })
                                         .ToListAsync();
            return Ok(teachers);
        }

        [HttpGet("{id}")] // GET /kawsay/teachers/{id}
        public async Task<ActionResult<Teacher>> GetTeacher(int id)
        {
            // Fetch entity and map to DTO
            var teacher = await _context.Teachers
                                        .Where(t => t.Id == id)
                                        .Select(t => new Teacher { Id = t.Id, Name = t.Name, Type = t.Type })
                                        .FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        [HttpPost] // POST /kawsay/teachers
         public async Task<ActionResult<Teacher>> CreateTeacher([FromBody] Teacher teacherDto) // Parameter name change
         {
             if (!ModelState.IsValid)
             {
                 return BadRequest(ModelState);
             }
              if (teacherDto.Type != "Professor" && teacherDto.Type != "Faculty Practitioner")
             {
                 return BadRequest(new { message = "Invalid teacher type. Must be 'Professor' or 'Faculty Practitioner'." });
             }

             // Map DTO to Entity
             var teacherEntity = new TeacherEntity { Name = teacherDto.Name, Type = teacherDto.Type };

             // Add to context and save
             _context.Teachers.Add(teacherEntity);
             await _context.SaveChangesAsync();

             // Map created Entity back to DTO
             var createdTeacherDto = new Teacher { Id = teacherEntity.Id, Name = teacherEntity.Name, Type = teacherEntity.Type };

             return CreatedAtAction(nameof(GetTeacher), new { id = createdTeacherDto.Id }, createdTeacherDto);
         }
    }
}

// Controllers/CoursesController.cs
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
    public class CoursesController : ControllerBase
    {
        private readonly KawsayDbContext _context; // Inject DbContext

        public CoursesController(KawsayDbContext context)
        {
            _context = context;
        }

        [HttpGet] // GET /kawsay/courses
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            // Fetch entities and map to DTOs
            var courses = await _context.Courses
                                        .Select(c => new Course { Id = c.Id, Name = c.Name, Code = c.Code })
                                        .ToListAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")] // GET /kawsay/courses/{id}
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            // Fetch entity and map to DTO
            var course = await _context.Courses
                                       .Where(c => c.Id == id)
                                       .Select(c => new Course { Id = c.Id, Name = c.Name, Code = c.Code })
                                       .FirstOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost] // POST /kawsay/courses
         public async Task<ActionResult<Course>> CreateCourse([FromBody] Course courseDto) // Parameter name change to avoid conflict
         {
             if (!ModelState.IsValid)
             {
                 return BadRequest(ModelState);
             }

             // Map DTO to Entity
             var courseEntity = new CourseEntity { Name = courseDto.Name, Code = courseDto.Code };

             // Add to context and save
             _context.Courses.Add(courseEntity);
             await _context.SaveChangesAsync(); // This assigns the database-generated ID to courseEntity.Id

             // Map created Entity back to DTO for response
             var createdCourseDto = new Course { Id = courseEntity.Id, Name = courseEntity.Name, Code = courseEntity.Code };

             return CreatedAtAction(nameof(GetCourse), new { id = createdCourseDto.Id }, createdCourseDto);
         }
    }
}

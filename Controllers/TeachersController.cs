// Controllers/TeachersController.cs
using KawsayApiMockup.Data;
using KawsayApiMockup.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace KawsayApiMockup.Controllers
{
    [ApiController]
    [Route("kawsay/[controller]")] // Base path /kawsay/teachers
    public class TeachersController : ControllerBase
    {
        [HttpGet] // GET /kawsay/teachers
        public ActionResult<IEnumerable<Teacher>> GetTeachers()
        {
            return Ok(MockData.Teachers);
        }

        // Added for API completeness, not used by current frontend
        [HttpGet("{id}")] // GET /kawsay/teachers/{id}
        public ActionResult<Teacher> GetTeacher(int id)
        {
            var teacher = MockData.Teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }

        // Added for API completeness, not used by current frontend
         [HttpPost] // POST /kawsay/teachers
         public ActionResult<Teacher> CreateTeacher([FromBody] Teacher teacher)
         {
             if (!ModelState.IsValid)
             {
                 return BadRequest(ModelState);
             }
             // In a real app, you'd add validation (e.g., valid type)
             if (teacher.Type != "Professor" && teacher.Type != "Faculty Practitioner")
             {
                 return BadRequest(new { message = "Invalid teacher type. Must be 'Professor' or 'Faculty Practitioner'." });
             }
             var createdTeacher = MockData.AddTeacher(teacher);
             return CreatedAtAction(nameof(GetTeacher), new { id = createdTeacher.Id }, createdTeacher);
         }
    }
}

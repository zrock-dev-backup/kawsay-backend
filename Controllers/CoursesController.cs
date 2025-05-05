// Controllers/CoursesController.cs
using KawsayApiMockup.Data;
using KawsayApiMockup.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace KawsayApiMockup.Controllers
{
    [ApiController]
    [Route("kawsay/[controller]")] // Base path /kawsay/courses
    public class CoursesController : ControllerBase
    {
        [HttpGet] // GET /kawsay/courses
        public ActionResult<IEnumerable<Course>> GetCourses()
        {
            return Ok(MockData.Courses);
        }

        // Added for API completeness, not used by current frontend
        [HttpGet("{id}")] // GET /kawsay/courses/{id}
        public ActionResult<Course> GetCourse(int id)
        {
            var course = MockData.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        // Added for API completeness, not used by current frontend
        [HttpPost] // POST /kawsay/courses
         public ActionResult<Course> CreateCourse([FromBody] Course course)
         {
             if (!ModelState.IsValid)
             {
                 return BadRequest(ModelState);
             }
             // In a real app, you'd add validation (e.g., unique code)
             var createdCourse = MockData.AddCourse(course);
             return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
         }
    }
}

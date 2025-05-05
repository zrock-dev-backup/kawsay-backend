// Controllers/TimetableController.cs
using KawsayApiMockup.Data;
using KawsayApiMockup.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace KawsayApiMockup.Controllers
{
    [ApiController]
    [Route("kawsay/[controller]")] // Base path /kawsay/timetable
    public class TimetableController : ControllerBase
    {
        [HttpPost] // POST /kawsay/timetable
        public ActionResult<TimetableStructure> CreateTimetable([FromBody] CreateTimetableRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Add more validation: check for duplicate period times, empty days/periods lists
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                 return BadRequest(new { message = "Timetable name is required." });
            }
             if (request.Days == null || request.Days.Count == 0)
             {
                  return BadRequest(new { message = "At least one day is required." });
             }
              if (request.Periods == null || request.Periods.Count == 0)
             {
                  return BadRequest(new { message = "At least one period is required." });
             }


            var createdTimetable = MockData.AddTimetable(request);

            // Return 201 Created with the location of the new resource
            return CreatedAtAction(nameof(GetTimetable), new { id = createdTimetable.Id }, createdTimetable);
        }

        [HttpGet("{id}")] // GET /kawsay/timetable/{id}
        public ActionResult<TimetableStructure> GetTimetable(int id)
        {
            var timetable = MockData.Timetables.FirstOrDefault(t => t.Id == id);
            if (timetable == null)
            {
                return NotFound();
            }
            return Ok(timetable);
        }

        // Added for API completeness, not used by current frontend
        [HttpGet] // GET /kawsay/timetables (List endpoint)
        [Route("/kawsay/timetables")] // Explicitly set route to /kawsay/timetables
        public ActionResult<IEnumerable<TimetableStructure>> GetTimetables()
        {
            return Ok(MockData.Timetables);
        }

        // Added for API completeness, not used by current frontend
        [HttpPut("{id}")] // PUT /kawsay/timetable/{id}
        public ActionResult<TimetableStructure> UpdateTimetable(int id, [FromBody] UpdateTimetableRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID in URL and body must match." });
            }
             if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
             if (string.IsNullOrWhiteSpace(request.Name))
            {
                 return BadRequest(new { message = "Timetable name is required." });
            }
             if (request.Days == null || request.Days.Count == 0)
             {
                  return BadRequest(new { message = "At least one day is required." });
             }
              if (request.Periods == null || request.Periods.Count == 0)
             {
                  return BadRequest(new { message = "At least one period is required." });
             }


            var updatedTimetable = MockData.UpdateTimetable(id, request);

            if (updatedTimetable == null)
            {
                return NotFound();
            }

            return Ok(updatedTimetable);
        }

        // Added for API completeness, not used by current frontend
        [HttpDelete("{id}")] // DELETE /kawsay/timetable/{id}
        public IActionResult DeleteTimetable(int id)
        {
            var deleted = MockData.DeleteTimetable(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent(); // 204 No Content is standard for successful deletion
        }
    }
}

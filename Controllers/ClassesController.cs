// Controllers/ClassesController.cs
using KawsayApiMockup.Data;
using KawsayApiMockup.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace KawsayApiMockup.Controllers
{
    [ApiController]
    [Route("kawsay")] // Base path /kawsay
    public class ClassesController : ControllerBase
    {
        [HttpGet("classes")] // GET /kawsay/classes?timetableId={id}
        public ActionResult<IEnumerable<Class>> GetClassesByTimetable([FromQuery] int timetableId)
        {
            // In a real app, validate if timetableId exists
             var timetableExists = MockData.Timetables.Any(t => t.Id == timetableId);
             if (!timetableExists)
             {
                 return NotFound(new { message = $"Timetable with ID {timetableId} not found." });
             }

            var classes = MockData.Classes.Where(c => c.TimetableId == timetableId).ToList();
            return Ok(classes);
        }

        [HttpGet("class/{id}")] // GET /kawsay/class/{id}
        public ActionResult<Class> GetClass(int id)
        {
            var cls = MockData.Classes.FirstOrDefault(c => c.Id == id);
            if (cls == null)
            {
                return NotFound();
            }
            return Ok(cls);
        }

        [HttpPost("class")] // POST /kawsay/class
        public ActionResult<Class> CreateClass([FromBody] CreateClassRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Basic validation (more needed in real app)
            if (request.Occurrences == null || request.Occurrences.Count == 0)
            {
                 return BadRequest(new { message = "At least one occurrence is required." });
            }
             if (request.Occurrences.Any(o => o.Length <= 0))
             {
                  return BadRequest(new { message = "Occurrence length must be positive." });
             }
             // Validate that CourseId exists
             if (!MockData.Courses.Any(c => c.Id == request.CourseId))
             {
                 return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });
             }
             // Validate that TeacherId exists if provided
             if (request.TeacherId.HasValue && !MockData.Teachers.Any(t => t.Id == request.TeacherId.Value))
             {
                 return BadRequest(new { message = $"Teacher with ID {request.TeacherId.Value} not found." });
             }
             // Validate that TimetableId exists
              var timetable = MockData.Timetables.FirstOrDefault(t => t.Id == request.TimetableId);
             if (timetable == null)
             {
                 return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });
             }
             // Validate that DayId and StartPeriodId exist within the specified Timetable
             foreach(var occ in request.Occurrences)
             {
                 if (!timetable.Days.Any(d => d.Id == occ.DayId))
                 {
                     return BadRequest(new { message = $"Day ID {occ.DayId} not found in timetable {request.TimetableId}." });
                 }
                  if (!timetable.Periods.Any(p => p.Id == occ.StartPeriodId))
                 {
                     return BadRequest(new { message = $"Period ID {occ.StartPeriodId} not found in timetable {request.TimetableId}." });
                 }
                 // Add validation for occurrence length not exceeding available periods after startPeriodId
                 var startPeriodIndex = timetable.Periods.FindIndex(p => p.Id == occ.StartPeriodId);
                 if (startPeriodIndex != -1 && startPeriodIndex + occ.Length > timetable.Periods.Count)
                 {
                      return BadRequest(new { message = $"Occurrence length {occ.Length} for period ID {occ.StartPeriodId} exceeds available periods in timetable {request.TimetableId}." });
                 }
                 // Add validation for scheduling conflicts! (This is complex for a mockup, but essential for a real app)
             }


            try
            {
                 var createdClass = MockData.AddClass(request);
                 // Return 201 Created with the location of the new resource
                 return CreatedAtAction(nameof(GetClass), new { id = createdClass.Id }, createdClass);
            }
            catch (System.ArgumentException ex)
            {
                // Catch specific validation errors from MockData.AddClass
                return BadRequest(new { message = ex.Message });
            }
             catch (System.Exception ex)
            {
                 // Catch any other unexpected errors
                 return StatusCode(500, new { message = "An internal error occurred while creating the class.", detail = ex.Message });
            }
        }

        // Added for API completeness, not used by current frontend
        [HttpPut("class/{id}")] // PUT /kawsay/class/{id}
        public ActionResult<Class> UpdateClass(int id, [FromBody] UpdateClassRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID in URL and body must match." });
            }
             if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
             if (request.Occurrences == null || request.Occurrences.Count == 0)
            {
                 return BadRequest(new { message = "At least one occurrence is required." });
            }
             if (request.Occurrences.Any(o => o.Length <= 0))
             {
                  return BadRequest(new { message = "Occurrence length must be positive." });
             }
             // Validate that CourseId exists
             if (!MockData.Courses.Any(c => c.Id == request.CourseId))
             {
                 return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });
             }
             // Validate that TeacherId exists if provided
             if (request.TeacherId.HasValue && !MockData.Teachers.Any(t => t.Id == request.TeacherId.Value))
             {
                 return BadRequest(new { message = $"Teacher with ID {request.TeacherId.Value} not found." });
             }
             // Validate that TimetableId exists and matches the existing class's timetable
              var existingClass = MockData.Classes.FirstOrDefault(c => c.Id == id);
              if (existingClass == null) return NotFound();
              if (existingClass.TimetableId != request.TimetableId)
              {
                  return BadRequest(new { message = $"Cannot change timetableId ({request.TimetableId}) for existing class ID {id}." });
              }
              var timetable = MockData.Timetables.FirstOrDefault(t => t.Id == request.TimetableId);
              if (timetable == null) // Should not happen if existingClass is found, but good practice
              {
                  return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });
              }

             // Validate that DayId and StartPeriodId exist within the specified Timetable for all occurrences
             foreach(var occ in request.Occurrences)
             {
                 if (!timetable.Days.Any(d => d.Id == occ.DayId))
                 {
                     return BadRequest(new { message = $"Day ID {occ.DayId} not found in timetable {request.TimetableId}." });
                 }
                  if (!timetable.Periods.Any(p => p.Id == occ.StartPeriodId))
                 {
                     return BadRequest(new { message = $"Period ID {occ.StartPeriodId} not found in timetable {request.TimetableId}." });
                 }
                  var startPeriodIndex = timetable.Periods.FindIndex(p => p.Id == occ.StartPeriodId);
                 if (startPeriodIndex != -1 && startPeriodIndex + occ.Length > timetable.Periods.Count)
                 {
                      return BadRequest(new { message = $"Occurrence length {occ.Length} for period ID {occ.StartPeriodId} exceeds available periods in timetable {request.TimetableId}." });
                 }
                 // Add validation for scheduling conflicts! (Excluding the occurrence being updated itself)
             }


            try
            {
                 var updatedClass = MockData.UpdateClass(id, request);
                 if (updatedClass == null)
                 {
                     return NotFound();
                 }
                 return Ok(updatedClass);
            }
             catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
             catch (System.Exception ex)
            {
                 return StatusCode(500, new { message = "An internal error occurred while updating the class.", detail = ex.Message });
            }
        }

        // Added for API completeness, not used by current frontend
        [HttpDelete("class/{id}")] // DELETE /kawsay/class/{id}
        public IActionResult DeleteClass(int id)
        {
            var deleted = MockData.DeleteClass(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent(); // 204 No Content
        }
    }
}

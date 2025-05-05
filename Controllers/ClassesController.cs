// Controllers/ClassesController.cs
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
    [Route("kawsay")] // Base path /kawsay
    public class ClassesController : ControllerBase
    {
        private readonly KawsayDbContext _context; // Inject DbContext

        public ClassesController(KawsayDbContext context)
        {
            _context = context;
        }


        [HttpGet("classes")] // GET /kawsay/classes?timetableId={id}
        public async Task<ActionResult<IEnumerable<Class>>> GetClassesByTimetable([FromQuery] int timetableId)
        {
            // In a real app, validate if timetableId exists (already done by EF Core relationship loading)
             var timetableExists = await _context.Timetables.AnyAsync(t => t.Id == timetableId);
             if (!timetableExists)
             {
                 return NotFound(new { message = $"Timetable with ID {timetableId} not found." });
             }

            // Fetch classes including related Course, Teacher, and Occurrences
            var classes = await _context.Classes
                                        .Include(c => c.Course)
                                        .Include(c => c.Teacher)
                                        .Include(c => c.Occurrences)
                                        .Where(c => c.TimetableId == timetableId)
                                        .ToListAsync();

            // Map entities to DTOs
            var classDtos = classes.Select(cls => new Class
            {
                Id = cls.Id,
                TimetableId = cls.TimetableId,
                Course = new Course { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
                Teacher = cls.Teacher != null ? new Teacher { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type } : null,
                Occurrences = cls.Occurrences.Select(o => new ClassOccurrence
                {
                    Id = o.Id,
                    DayId = o.DayId,
                    StartPeriodId = o.StartPeriodId,
                    Length = o.Length
                }).ToList()
            }).ToList();

            return Ok(classDtos);
        }

        [HttpGet("class/{id}")] // GET /kawsay/class/{id}
        public async Task<ActionResult<Class>> GetClass(int id)
        {
            // Fetch class including related Course, Teacher, and Occurrences
            var cls = await _context.Classes
                                    .Include(c => c.Course)
                                    .Include(c => c.Teacher)
                                    .Include(c => c.Occurrences)
                                    .Where(c => c.Id == id)
                                    .FirstOrDefaultAsync();

            if (cls == null)
            {
                return NotFound();
            }

            // Map entity to DTO
             var classDto = new Class
            {
                Id = cls.Id,
                TimetableId = cls.TimetableId,
                Course = new Course { Id = cls.Course.Id, Name = cls.Course.Name, Code = cls.Course.Code },
                Teacher = cls.Teacher != null ? new Teacher { Id = cls.Teacher.Id, Name = cls.Teacher.Name, Type = cls.Teacher.Type } : null,
                Occurrences = cls.Occurrences.Select(o => new ClassOccurrence
                {
                    Id = o.Id,
                    DayId = o.DayId,
                    StartPeriodId = o.StartPeriodId,
                    Length = o.Length
                }).ToList()
            };

            return Ok(classDto);
        }

        [HttpPost("class")] // POST /kawsay/class
        public async Task<ActionResult<Class>> CreateClass([FromBody] CreateClassRequest request)
        {
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

            // Validate existence of related entities
            var course = await _context.Courses.FindAsync(request.CourseId);
            if (course == null)
            {
                return BadRequest(new { message = $"Course with ID {request.CourseId} not found." });
            }

            TeacherEntity? teacher = null;
            if (request.TeacherId.HasValue)
            {
                teacher = await _context.Teachers.FindAsync(request.TeacherId.Value);
                if (teacher == null)
                {
                    return BadRequest(new { message = $"Teacher with ID {request.TeacherId.Value} not found." });
                }
            }

             var timetable = await _context.Timetables
                                           .Include(t => t.Days) // Include days/periods for validation
                                           .Include(t => t.Periods)
                                           .Where(t => t.Id == request.TimetableId)
                                           .FirstOrDefaultAsync();
             if (timetable == null)
             {
                 return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });
             }

             // Validate occurrence details against the timetable structure
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
                 var startPeriodIndex = timetable.Periods.OrderBy(p => p.Start).ToList().FindIndex(p => p.Id == occ.StartPeriodId); // Ensure periods are ordered for length check
                 if (startPeriodIndex != -1 && startPeriodIndex + occ.Length > timetable.Periods.Count)
                 {
                      return BadRequest(new { message = $"Occurrence length {occ.Length} for period ID {occ.StartPeriodId} exceeds available periods in timetable {request.TimetableId}." });
                 }
                 // TODO: Add validation for scheduling conflicts! (Requires querying existing classes in the timetable)
             }


            // Map request DTO to Entity
            var classEntity = new ClassEntity
            {
                TimetableId = request.TimetableId,
                CourseId = request.CourseId, // Use foreign key ID
                TeacherId = request.TeacherId, // Use foreign key ID (nullable)
                Occurrences = request.Occurrences.Select(o => new ClassOccurrenceEntity
                {
                    DayId = o.DayId,
                    StartPeriodId = o.StartPeriodId,
                    Length = o.Length
                }).ToList() // IDs will be generated by DB
            };

            // Add to context and save
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync(); // Saves Class and Occurrences

            // Re-fetch the created entity with navigation properties to return the full DTO
            var createdClassEntity = await _context.Classes
                                                   .Include(c => c.Course)
                                                   .Include(c => c.Teacher)
                                                   .Include(c => c.Occurrences)
                                                   .Where(c => c.Id == classEntity.Id)
                                                   .FirstOrDefaultAsync();

            // Map created Entity back to DTO for response
             var createdClassDto = new Class
            {
                Id = createdClassEntity!.Id, // Use the ID generated by the DB
                TimetableId = createdClassEntity.TimetableId,
                Course = new Course { Id = createdClassEntity.Course.Id, Name = createdClassEntity.Course.Name, Code = createdClassEntity.Course.Code },
                Teacher = createdClassEntity.Teacher != null ? new Teacher { Id = createdClassEntity.Teacher.Id, Name = createdClassEntity.Teacher.Name, Type = createdClassEntity.Teacher.Type } : null,
                Occurrences = createdClassEntity.Occurrences.Select(o => new ClassOccurrence { Id = o.Id, DayId = o.DayId, StartPeriodId = o.StartPeriodId, Length = o.Length }).ToList()
            };


            return CreatedAtAction(nameof(GetClass), new { id = createdClassDto.Id }, createdClassDto);
        }

        // Added for API completeness, not used by current frontend
        [HttpPut("class/{id}")] // PUT /kawsay/class/{id}
        public async Task<ActionResult<Class>> UpdateClass(int id, [FromBody] UpdateClassRequest request)
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

            // Fetch the existing class including related data needed for update and validation
            var existingClass = await _context.Classes
                                              .Include(c => c.Course)
                                              .Include(c => c.Teacher)
                                              .Include(c => c.Occurrences) // Include existing occurrences
                                              .Where(c => c.Id == id)
                                              .FirstOrDefaultAsync();

            if (existingClass == null)
            {
                return NotFound();
            }

             // Validate timetableId matches the existing one
             if (existingClass.TimetableId != request.TimetableId)
             {
                 return BadRequest(new { message = $"Cannot change timetableId ({request.TimetableId}) for existing class ID {id}." });
             }

             // Validate existence of related entities (Course, Teacher)
             var course = await _context.Courses.FindAsync(request.CourseId);
             if (course == null)
             {
                 return BadRequest(new { message = $"Course with ID {request.CourseId} not found during update." });
             }

             TeacherEntity? teacher = null;
             if (request.TeacherId.HasValue)
             {
                 teacher = await _context.Teachers.FindAsync(request.TeacherId.Value);
                 if (teacher == null)
                 {
                     return BadRequest(new { message = $"Teacher with ID {request.TeacherId.Value} not found during update." });
                 }
             }

             // Fetch timetable structure for occurrence validation
             var timetable = await _context.Timetables
                                           .Include(t => t.Days)
                                           .Include(t => t.Periods)
                                           .Where(t => t.Id == request.TimetableId)
                                           .FirstOrDefaultAsync();
             if (timetable == null) // Should not happen if existingClass is found, but defensive
             {
                 return BadRequest(new { message = $"Timetable with ID {request.TimetableId} not found." });
             }

             // Validate new/updated occurrence details against the timetable structure
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
                 var startPeriodIndex = timetable.Periods.OrderBy(p => p.Start).ToList().FindIndex(p => p.Id == occ.StartPeriodId);
                 if (startPeriodIndex != -1 && startPeriodIndex + occ.Length > timetable.Periods.Count)
                 {
                      return BadRequest(new { message = $"Occurrence length {occ.Length} for period ID {occ.StartPeriodId} exceeds available periods in timetable {request.TimetableId}." });
                 }
                 // TODO: Add validation for scheduling conflicts! (Excluding the occurrence being updated)
             }


            // Apply updates to the existing entity
            existingClass.CourseId = request.CourseId; // Update foreign key
            existingClass.TeacherId = request.TeacherId; // Update foreign key (nullable)

            // Update nested occurrences: compare existing with requested
            // This is a common pattern for replacing/updating nested collections in EF Core
            var existingOccurrences = existingClass.Occurrences.ToList(); // Make a copy to modify the original collection
            var requestedOccurrences = request.Occurrences.ToList();

            // Remove occurrences that are in existing but not in requested
            foreach (var existingOcc in existingOccurrences)
            {
                if (!requestedOccurrences.Any(reqOcc => reqOcc.Id == existingOcc.Id && reqOcc.Id != 0)) // Check by ID for existing ones
                {
                    _context.ClassOccurrences.Remove(existingOcc);
                }
            }

            // Add or Update occurrences from requested
            foreach (var requestedOcc in requestedOccurrences)
            {
                if (requestedOcc.Id == 0) // ID == 0 typically means new entity
                {
                    // Add new occurrence
                     existingClass.Occurrences.Add(new ClassOccurrenceEntity {
                         DayId = requestedOcc.DayId,
                         StartPeriodId = requestedOcc.StartPeriodId,
                         Length = requestedOcc.Length
                         // ClassId is set automatically by EF Core when added to the collection
                     });
                }
                else
                {
                    // Find existing occurrence to update
                    var existingOcc = existingOccurrences.FirstOrDefault(eo => eo.Id == requestedOcc.Id);
                    if (existingOcc != null)
                    {
                        // Update properties
                        existingOcc.DayId = requestedOcc.DayId;
                        existingOcc.StartPeriodId = requestedOcc.StartPeriodId;
                        existingOcc.Length = requestedOcc.Length;
                        _context.ClassOccurrences.Update(existingOcc); // Mark as modified
                    }
                    else
                    {
                         // This case means an ID was provided in the request body, but it didn't exist for this class
                         // Depending on requirements, you might return BadRequest or ignore/add it.
                         // For this mockup, let's return BadRequest.
                         return BadRequest(new { message = $"Occurrence with ID {requestedOcc.Id} not found for class {id}." });
                    }
                }
            }


            await _context.SaveChangesAsync(); // Saves all changes (removals, additions, updates)

            // Re-fetch the updated entity with navigation properties to return the full DTO
            var updatedClassEntity = await _context.Classes
                                                   .Include(c => c.Course)
                                                   .Include(c => c.Teacher)
                                                   .Include(c => c.Occurrences)
                                                   .Where(c => c.Id == id)
                                                   .FirstOrDefaultAsync();

            // Map updated Entity back to DTO for response
             var updatedClassDto = new Class
            {
                Id = updatedClassEntity!.Id,
                TimetableId = updatedClassEntity.TimetableId,
                Course = new Course { Id = updatedClassEntity.Course.Id, Name = updatedClassEntity.Course.Name, Code = updatedClassEntity.Course.Code },
                Teacher = updatedClassEntity.Teacher != null ? new Teacher { Id = updatedClassEntity.Teacher.Id, Name = updatedClassEntity.Teacher.Name, Type = updatedClassEntity.Teacher.Type } : null,
                Occurrences = updatedClassEntity.Occurrences.Select(o => new ClassOccurrence { Id = o.Id, DayId = o.DayId, StartPeriodId = o.StartPeriodId, Length = o.Length }).ToList()
            };


            return Ok(updatedClassDto);
        }

        // Added for API completeness, not used by current frontend
        [HttpDelete("class/{id}")] // DELETE /kawsay/class/{id}
        public async Task<IActionResult> DeleteClass(int id)
        {
            var cls = await _context.Classes.FindAsync(id); // Find by primary key
            if (cls == null)
            {
                return NotFound();
            }

            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync(); // Cascade delete occurrences handled by OnModelCreating

            return NoContent(); // 204 No Content
        }
    }
}

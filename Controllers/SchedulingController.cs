// Controllers/SchedulingController.cs

using KawsayApiMockup.Services; // Import the Scheduling Service
using Microsoft.AspNetCore.Mvc;
using System; // Import for ArgumentException
using System.Threading.Tasks;

namespace KawsayApiMockup.Controllers // Ensure correct namespace for your project
{
    [ApiController]
    [Route("kawsay/[controller]")] // Base path /kawsay/scheduling
    public class SchedulingController : ControllerBase
    {
        private readonly SchedulingService _schedulingService;

        public SchedulingController(SchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
        }

        /// <summary>
        /// Triggers the schedule generation process for a specific timetable using the Yule algorithm.
        /// </summary>
        /// <param name="timetableId">The ID of the timetable to schedule.</param>
        /// <returns>An IActionResult indicating the result of the generation process.</returns>
        [HttpPost("generate/{timetableId}")] // POST /kawsay/scheduling/generate/{timetableId}
        public async Task<IActionResult> GenerateSchedule(int timetableId)
        {
            try
            {
                // Call the SchedulingService to generate the schedule
                bool success = await _schedulingService.GenerateScheduleAsync(timetableId);

                if (success)
                {
                    // If the service returns true, it means the algorithm completed within attempts.
                    // This implies a schedule was found (potentially incomplete if all requirements weren't satisfied).
                    return Ok(new { message = $"Schedule generation process completed for timetable ID {timetableId}. Check timetable grid for results." });
                }
                else
                {
                    // If the service returns false, it means the algorithm failed to complete
                    // within the maximum number of attempts.
                    return Conflict(new { message = $"Schedule generation failed to find a complete solution for timetable ID {timetableId} within the allowed attempts. Constraints may be too tight." });
                }
            }
            catch (ArgumentException ex)
            {
                // Handle specific validation errors thrown by the service (e.g., timetable not found)
                 if (ex.Message.Contains("Timetable with ID") && ex.Message.Contains("not found"))
                 {
                      // Return 404 if the timetable ID was not found
                      return NotFound(new { message = ex.Message });
                 }
                 // For other ArgumentExceptions (e.g., invalid input data structure if we added more validation)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors during the process
                System.Console.WriteLine($"Internal Server Error during scheduling: {ex}"); // Log the detailed error server-side
                return StatusCode(500, new { message = "An internal error occurred during schedule generation.", detail = ex.Message });
            }
        }

        // You might add other scheduling-related endpoints here later, e.g.,
        // - GET /kawsay/scheduling/status/{timetableId} (Check if a schedule is currently being generated)
        // - POST /kawsay/scheduling/validate/{timetableId} (Run validation without saving)
        // - DELETE /kawsay/scheduling/{timetableId} (Clear the generated schedule)
    }
}

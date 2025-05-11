using kawsay.Services;
using Microsoft.AspNetCore.Mvc;

namespace kawsay.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class SchedulingController(SchedulingService schedulingService) : ControllerBase
{
    [HttpPost("generate/{timetableId}")]
    public async Task<IActionResult> GenerateSchedule(int timetableId)
    {
        try
        {
            var success = await schedulingService.GenerateScheduleAsync(timetableId);

            if (success)
                return Ok(new
                {
                    message =
                        $"Schedule generation process completed for timetable ID {timetableId}. Check timetable grid for results."
                });

            return Conflict(new
            {
                message =
                    $"Schedule generation failed to find a complete solution for timetable ID {timetableId} within the allowed attempts. Constraints may be too tight."
            });
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("Timetable with ID") && ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal Server Error during scheduling: {ex}");
            return StatusCode(500,
                new { message = "An internal error occurred during schedule generation.", detail = ex.Message });
        }
    }
}
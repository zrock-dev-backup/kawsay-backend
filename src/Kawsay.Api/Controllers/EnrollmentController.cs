using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/enrollments")]
public class EnrollmentController(EnrollmentService enrollmentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEnrollment([FromBody] EnrollmentRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var enrollment = await enrollmentService.EnrollStudentAsync(request);
            return Ok(enrollment);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "An unexpected error occurred during enrollment." });
        }
    }
}

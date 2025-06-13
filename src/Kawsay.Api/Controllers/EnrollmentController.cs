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
            var responseDto = new EnrollmentResponseDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                ClassId = enrollment.ClassId,
                EnrollmentDate = enrollment.EnrollmentDate
            };
            return Ok(responseDto);
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

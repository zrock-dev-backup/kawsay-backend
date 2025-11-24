using Application.Core;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class StudentAuditController(IStudentAuditService auditService) : ControllerBase
{
    [HttpGet("kawsay/timetables/{timetableId:int}/student-audit")]
    [ProducesResponseType(typeof(IEnumerable<StudentAuditDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentAudit(int timetableId)
    {
        var result = await auditService.GetStudentAuditAsync(timetableId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("kawsay/timetables/{timetableId:int}/bulk-enroll")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BulkEnroll(int timetableId, [FromBody] BulkEnrollmentRequest request)
    {
        if (timetableId != request.TimetableId) return BadRequest("Timetable ID mismatch");
        
        var result = await auditService.BulkEnrollAsync(request);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

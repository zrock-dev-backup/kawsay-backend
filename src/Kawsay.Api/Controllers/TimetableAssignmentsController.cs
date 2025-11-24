using Application.Core;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/timetables/{timetableId:int}/assignments")]
public class TimetableAssignmentsController(ITimetableAssignmentService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TimetableAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments(int timetableId)
    {
        var result = await service.GetAssignmentsAsync(timetableId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TimetableAssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssignment(int timetableId, [FromBody] CreateTimetableAssignmentRequestDto request)
    {
        var result = await service.CreateAssignmentAsync(timetableId, request);
        if (result.IsSuccess)
        {
            // We don't have a specific GetById endpoint for assignments, so we use the list endpoint for location
            return CreatedAtAction(nameof(GetAssignments), new { timetableId }, result.Value);
        }
        
        return result.Error.Type == ErrorType.NotFound 
            ? NotFound(new { result.Error.Message }) 
            : BadRequest(new { result.Error.Message });
    }

    [HttpDelete("{assignmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssignment(int timetableId, int assignmentId)
    {
        var result = await service.DeleteAssignmentAsync(assignmentId);
        return result.IsSuccess ? NoContent() : NotFound(new { result.Error.Message });
    }
}

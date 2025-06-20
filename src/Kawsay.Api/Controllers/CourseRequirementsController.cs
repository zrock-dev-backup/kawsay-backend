using Application.Core;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay")]
public class CourseRequirementsController(ICourseRequirementService requirementService) : ControllerBase
{
    [HttpPost("timetables/{timetableId:int}/course-requirements")]
    public async Task<IActionResult> CreateCourseRequirement(int timetableId, [FromBody] CreateCourseRequirementRequestDto request)
    {
        var result = await requirementService.CreateCourseRequirementAsync(timetableId, request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCourseRequirement), new { requirementId = result.Value!.Id }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpGet("course-requirements/{requirementId:int}")]
    public async Task<IActionResult> GetCourseRequirement(int requirementId)
    {
        var result = await requirementService.GetCourseRequirementByIdAsync(requirementId);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("timetables/{timetableId:int}/course-requirements")]
    public async Task<IActionResult> GetCourseRequirementsForTimetable(int timetableId)
    {
        var result = await requirementService.GetCourseRequirementsForTimetableAsync(timetableId);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPut("course-requirements/{requirementId:int}")]
    public async Task<IActionResult> UpdateCourseRequirement(int requirementId, [FromBody] UpdateCourseRequirementRequestDto request)
    {
        var result = await requirementService.UpdateCourseRequirementAsync(requirementId, request);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpDelete("course-requirements/{requirementId:int}")]
    public async Task<IActionResult> DeleteCourseRequirement(int requirementId)
    {
        var result = await requirementService.DeleteCourseRequirementAsync(requirementId);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPost("course-requirements/{requirementId:int}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int requirementId, [FromBody] AvailableSlotsRequestDto request)
    {
        var result = await requirementService.GetAvailableSlotsForRequirementAsync(requirementId, request);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    private ObjectResult HandleFailure(Error error) => error.Type switch
    {
        ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
        ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Conflict(new { error.Code, error.Message }),
        _ => StatusCode(500, new { error.Code, error.Message })
    };
}

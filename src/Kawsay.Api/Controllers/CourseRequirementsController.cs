using Application.Core;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/requirements")]
public class CourseRequirementsController(ICourseRequirementService requirementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CourseRequirementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequirements([FromQuery] int timetableId)
    {
        if (timetableId <= 0) return BadRequest(new { message = "timetableId is required." });

        var result = await requirementService.GetCourseRequirementsForTimetableAsync(timetableId);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CourseRequirementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequirement(int id)
    {
        var result = await requirementService.GetCourseRequirementByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseRequirementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequirement([FromBody] CreateCourseRequirementRequestDto request)
    {
        var result = await requirementService.CreateCourseRequirementAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetRequirement), new { id = result.Value!.Id }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CourseRequirementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRequirement(int id, [FromBody] UpdateCourseRequirementRequestDto request)
    {
        var result = await requirementService.UpdateCourseRequirementAsync(id, request);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRequirement(int id)
    {
        var result = await requirementService.DeleteCourseRequirementAsync(id);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    private ObjectResult HandleFailure(Error error) => error.Type switch
    {
        ErrorType.Validation => BadRequest(new { error.Code, error.Message }),
        ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Conflict(new { error.Code, error.Message }),
        _ => StatusCode(500, new { error.Code, error.Message })
    };
}

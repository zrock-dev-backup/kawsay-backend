using Application.Models.Faculty;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/faculty")]
public class FacultyController(FacultyAvailabilityService availabilityService) : ControllerBase
{
    [HttpPost("{facultyId:int}/availability")]
    [ProducesResponseType(typeof(List<AvailabilityResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAvailabilities(int facultyId, [FromBody] List<AvailabilityRequestDto> requests)
    {
        var result = await availabilityService.AddAvailabilitiesAsync(facultyId, requests);
        
        return result.IsFailure ? BadRequest(result.Error) :
            // Return 201 Created and optionally set Location header (omitted for brevity)
            StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{facultyId:int}/availability")]
    [ProducesResponseType(typeof(List<AvailabilityResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFacultyAvailability(int facultyId)
    {
        var result = await availabilityService.GetByFacultyIdAsync(facultyId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("availability")]
    [ProducesResponseType(typeof(List<FacultyAvailabilityGroupResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAvailability()
    {
        var result = await availabilityService.GetAllGroupedAsync();
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{facultyId:int}/availability/{availabilityId:int}")]
    [ProducesResponseType(typeof(AvailabilityResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAvailability(int facultyId, int availabilityId, [FromBody] AvailabilityRequestDto request)
    {
        var result = await availabilityService.UpdateAsync(facultyId, availabilityId, request);
        
        if (result.IsFailure)
        {
             return result.Error.Type == Application.Core.ErrorType.NotFound 
                 ? NotFound(result.Error) 
                 : BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }

    [HttpPatch("{facultyId:int}/availability/{availabilityId:int}")]
    [ProducesResponseType(typeof(AvailabilityResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PatchAvailability(int facultyId, int availabilityId, [FromBody] AvailabilityPatchRequestDto request)
    {
        var result = await availabilityService.PatchAsync(facultyId, availabilityId, request);
        
        if (result.IsFailure)
        {
             return result.Error.Type == Application.Core.ErrorType.NotFound 
                 ? NotFound(result.Error) 
                 : BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }

    [HttpDelete("{facultyId:int}/availability/{availabilityId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAvailability(int facultyId, int availabilityId)
    {
        var result = await availabilityService.DeleteAsync(facultyId, availabilityId);
        
        if (result.IsFailure)
        {
             return result.Error.Type == Application.Core.ErrorType.NotFound 
                 ? NotFound(result.Error) 
                 : BadRequest(result.Error);
        }
        
        return NoContent();
    }
}

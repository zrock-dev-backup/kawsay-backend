using Application.Core;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/scheduling")]
public class SchedulingController(SchedulingService service) : ControllerBase
{
    [HttpPost("stage")]
    [ProducesResponseType(typeof(StagedPlacementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> StagePlacement([FromBody] CreateStagedPlacementRequest request)
    {
        var result = await service.StagePlacementAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("stage/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnstagePlacement(int id)
    {
        var result = await service.DeleteStagedPlacementAsync(id);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("finalize")]
    [ProducesResponseType(typeof(FinalizeScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Finalize([FromBody] FinalizeScheduleRequest request)
    {
        var result = await service.FinalizeScheduleAsync(request.TimetableId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

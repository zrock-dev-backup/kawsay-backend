using Application.Core;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/predictions")]
public class PredictionsController(IPredictionService predictionService) : ControllerBase
{
    [HttpPost("batch")]
    [ProducesResponseType(typeof(List<StudentPredictionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPredictions([FromBody] List<StudentGradeInputDto> request)
    {
        if (request.Count == 0)
            return BadRequest(new { message = "Request list cannot be empty." });

        var result = await predictionService.PredictBatchAsync(request);

        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

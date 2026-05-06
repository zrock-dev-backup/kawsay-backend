using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/machine")]
public class TimetableMachineController(
    Stage0ConfigurationService stage0,
    Stage1RelationalMappingService stage1,
    Stage2ActivityService stage2) : ControllerBase
{
    // --- STAGE 0: Configuration ---
    [HttpPost("stage0/timetables")]
    [ProducesResponseType(typeof(TimetableCreatedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TimetableCreatedResponse>> CreateTimetable([FromBody] TimetableCreateDto request)
    {
        var result = await stage0.CreateTimetableAsync(request);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new TimetableCreatedResponse { TimetableId = result.Value });
    }

    // --- STAGE 2: Activity Modelling ---
    [HttpPost("stage2/timetables/{timetableId}/activities")]
    [ProducesResponseType(typeof(ActivityCreatedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActivityCreatedResponse>> CreateActivity(int timetableId,
        [FromBody] CourseRequirementCreateDto request)
    {
        var result = await stage2.CreateActivityAsync(timetableId, request);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new ActivityCreatedResponse { ActivityId = result.Value });
    }

    // --- STAGE 3: Generation ---
    [HttpPost("stage3/timetables/{timetableId}/generate")]
    [ProducesResponseType(typeof(GeneratedTimetableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneratedTimetableDto>> GenerateTimetable(
        int timetableId,
        [FromServices] TimetableGenerationService generationService,
        CancellationToken cancellationToken)
    {
        var result = await generationService.GenerateTimetableAsync(timetableId, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("stage0/timetables")]
    [ProducesResponseType(typeof(List<TimetableSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TimetableSummaryDto>>> GetTimetables()
    {
        var result = await stage0.GetAllTimetablesAsync();

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("stage0/timetables/{timetableId:int}")]
    [ProducesResponseType(typeof(TimetableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Core.Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TimetableDto>> GetTimetable(int timetableId)
    {
        var result = await stage0.GetTimetableByIdAsync(timetableId);

        if (!result.IsSuccess)
        {
            if (result.Error.Type == Application.Core.ErrorType.NotFound)
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("stage2/timetables/{timetableId}/activities")]
    [ProducesResponseType(typeof(List<ActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ActivityDto>>> GetActivities(int timetableId)
    {
        var result = await stage2.GetActivitiesAsync(timetableId);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("stage3/timetables/{timetableId}/schedule")]
    [ProducesResponseType(typeof(List<GeneratedClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GeneratedClassDto>>> GetSchedule(
        int timetableId,
        [FromServices] TimetableGenerationService generationService)
    {
        var result = await generationService.GetScheduleAsync(timetableId);

        if (!result.IsSuccess)
        {
            if (result.Error.Type == Application.Core.ErrorType.NotFound)
                return NotFound(result.Error);
            
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}

public class TimetableCreatedResponse
{
    public int TimetableId { get; set; }
}

public class ActivityCreatedResponse
{
    public int ActivityId { get; set; }
}
using Application.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/timetables/{timetableId:int}/exceptions")]
public class ExceptionResolutionController(
    ICourseRequirementRepository requirementRepo,
    IStagedPlacementRepository placementRepo) : ControllerBase
{
    /// <summary>
    /// Returns all Course Requirements that were sent to the solver but resulted in NO staged placements.
    /// This drives the manual Stage 2 <-> 3 feedback loop.
    /// </summary>
    [HttpGet("unplaced-requirements")]
    [ProducesResponseType(typeof(List<UnplacedRequirementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnplacedRequirements(int timetableId)
    {
        var allRequirements = await requirementRepo.GetByTimetableIdAsync(timetableId);
        var successfulPlacements = await placementRepo.GetByTimetableIdAsync(timetableId);
        
        var placedRequirementIds = successfulPlacements
            .Select(p => p.CourseRequirementId)
            .ToHashSet();

        var unplaced = allRequirements
            .Where(r => !placedRequirementIds.Contains(r.Id))
            .Select(r => new UnplacedRequirementDto(
                r.Id,
                r.Course.Name,
                r.Course.Code,
                r.Priority.ToString(),
                r.DurationInPeriods,
                "Solver could not find a feasible slot given current constraints."
            ))
            .ToList();

        return Ok(unplaced);
    }
}

public record UnplacedRequirementDto(
    int RequirementId,
    string CourseName,
    string CourseCode,
    string Priority,
    int RequestedDuration,
    string Reason
);

using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/academic-structure")]
public class AcademicStructureController(
    AcademicStructureService structureService,
    RosterSyncService rosterSyncService
) : ControllerBase
{
    // --- COHORT ENDPOINTS ---

    [HttpGet("cohorts/{cohortId:int}")]
    public async Task<ActionResult<CohortDetailDto>> GetCohort(int cohortId)
    {
        var cohort = await structureService.GetCohortDetailsAsync(cohortId);
        return cohort == null ? NotFound(new { message = $"Cohort with ID {cohortId} not found." }) : Ok(cohort);
    }

    // --- SYNC ENDPOINTS ---
    // TODO: should move under timetables
    [HttpPost("~/kawsay/timetables/{timetableId:int}/academic-structure/sync")]
    [ProducesResponseType(typeof(AcademicStructureSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncRoster(int timetableId, [FromQuery] string externalCohortId)
    {
        if (string.IsNullOrWhiteSpace(externalCohortId))
            return BadRequest(new { message = "The 'externalCohortId' query parameter is required." });

        var result = await rosterSyncService.SyncRosterAsync(timetableId, externalCohortId);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { message = result.Error.Message });
    }

    [HttpGet("~/kawsay/timetables/{timetableId:int}/cohorts-summary")]
    public async Task<IActionResult> GetCohortsSummary(int timetableId)
    {
        var cohorts = await structureService.GetCohortsByTimetableAsync(timetableId);
        return Ok(cohorts.Select(c => new { c.Id, c.Name }));
    }

    [HttpGet("cohorts/{cohortId:int}/groups-summary")]
    public async Task<IActionResult> GetGroupsSummary(int cohortId)
    {
        var cohort = await structureService.GetCohortDetailsAsync(cohortId);
        if (cohort == null) return NotFound();
        return Ok(cohort.StudentGroups.Select(g => new { g.Id, g.Name }));
    }

    [HttpGet("groups/{groupId:int}/sections-summary")]
    public async Task<IActionResult> GetSectionsSummary(int groupId)
    {
        var group = await structureService.GetStudentGroupByIdAsync(groupId);
        if (group == null) return NotFound();
        return Ok(group.Sections.Select(s => new { s.Id, s.Name }));
    }
}

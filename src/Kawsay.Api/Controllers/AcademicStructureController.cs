using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/academic-structure")]
public class AcademicStructureController(AcademicStructureService structureService) : ControllerBase
{
    // --- COHORT ENDPOINTS ---

    [HttpPost("cohorts")]
    public async Task<ActionResult<CohortDetailDto>> CreateCohort([FromBody] CreateCohortRequest request)
    {
        try
        {
            var cohort = await structureService.CreateCohortAsync(request);
            return CreatedAtAction(nameof(GetCohort), new { cohortId = cohort!.Id }, cohort);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("cohorts/{cohortId:int}")]
    public async Task<ActionResult<CohortDetailDto>> GetCohort(int cohortId)
    {
        var cohort = await structureService.GetCohortDetailsAsync(cohortId);
        return cohort == null ? NotFound() : Ok(cohort);
    }
    
    // --- STUDENT GROUP ENDPOINTS ---
    
    [HttpPost("groups")]
    public async Task<IActionResult> CreateStudentGroup([FromBody] CreateStudentGroupRequest request)
    {
        try
        {
            var group = await structureService.CreateStudentGroupAsync(request);
            return Ok(group);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    // --- SECTION ENDPOINTS ---

    [HttpPost("sections")]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionRequest request)
    {
        try
        {
            var section = await structureService.CreateSectionAsync(request);
            return Ok(section);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // --- ASSIGNMENT ENDPOINT ---
    
    [HttpPost("sections/students")]
    public async Task<IActionResult> AssignStudentToSection([FromBody] AssignStudentToSectionRequest request)
    {
        try
        {
            await structureService.AssignStudentToSectionAsync(request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/module-processing/{timetableId:int}")]
public class ModuleProcessingController(EndofModuleService endofModuleService) : ControllerBase
{
    [HttpPost("ingest-grades")]
    public async Task<IActionResult> IngestGrades(int timetableId, [FromBody] List<GradeIngestionDto> gradeData)
    {
        if (!ModelState.IsValid || !gradeData.Any())
        {
            return BadRequest(new { message = "Invalid or empty grade data provided." });
        }

        try
        {
            await endofModuleService.IngestGradesAsync(timetableId, gradeData);
            return Ok(new { message = $"Successfully ingested {gradeData.Count} grade records for timetable {timetableId}." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "An internal error occurred while ingesting grades." });
        }
    }

    [HttpGet("cohorts")]
    public async Task<ActionResult<StudentCohortDto>> GetCohorts(int timetableId)
    {
        try
        {
            var cohorts = await endofModuleService.SegmentCohortsAsync(timetableId);
            return Ok(cohorts);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "An internal error occurred while segmenting cohorts." });
        }
    }
}

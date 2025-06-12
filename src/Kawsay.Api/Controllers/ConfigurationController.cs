using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/configuration")]
public class ConfigurationController(ConfigurationService configService) : ControllerBase
{
    [HttpGet("class-type-defaults")]
    public async Task<ActionResult<IEnumerable<ClassTypeConfigurationDto>>> GetAllClassTypeConfigurations()
    {
        var configs = await configService.GetAllClassTypeConfigurationsAsync();
        return Ok(configs);
    }
    
    [HttpPost("class-type-defaults")]
    public async Task<IActionResult> SetClassTypeConfigurations([FromBody] List<SetClassTypeConfigurationRequest> request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await configService.SetClassTypeConfigurationsAsync(request);
            return Ok(new { message = "Class type configurations updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("teacher-qualifications")]
    public async Task<IActionResult> AddTeacherQualifications([FromBody] CreateTeacherQualificationRequest request)
    {
        try
        {
            await configService.AddTeacherQualificationsAsync(request);
            return Ok(new { message = "Teacher qualifications added successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

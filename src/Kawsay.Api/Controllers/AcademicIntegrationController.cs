using Application.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Kawsay.Api.Controllers;

/// <summary>
/// Diagnostic controller to test the integration with the External Academic Mock API.
/// </summary>
[ApiController]
[Route("api/integration/academic")]
public class AcademicIntegrationController : ControllerBase
{
    private readonly IAcademicApiClient _academicApiClient;

    public AcademicIntegrationController(IAcademicApiClient academicApiClient)
    {
        _academicApiClient = academicApiClient;
    }

    [HttpGet("catalog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
    {
        var result = await _academicApiClient.GetCatalogAsync(cancellationToken);
        return result is not null ? Ok(result) : NoContent();
    }

    [HttpGet("catalog/courses/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourse(string code, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _academicApiClient.GetCourseAsync(code, cancellationToken);
            return result is not null ? Ok(result) : NotFound($"Course {code} not found.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound($"Course {code} not found in external system.");
        }
    }

    [HttpGet("cohorts/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCohort(string id, CancellationToken cancellationToken)
    {
        var result = await _academicApiClient.GetCohortAsync(id, cancellationToken);
        return result is not null ? Ok(result) : NotFound($"Cohort {id} not found.");
    }

    [HttpGet("students/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudent(string id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _academicApiClient.GetStudentProfileAsync(id, cancellationToken);
            return result is not null ? Ok(result) : NotFound($"Student {id} not found.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound($"Student {id} not found in external system.");
        }
    }
}
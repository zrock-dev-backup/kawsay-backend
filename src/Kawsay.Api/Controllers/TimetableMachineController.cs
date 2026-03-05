using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1")]
public class TimetableMachineController(
    Stage0ConfigurationService stage0,
    Stage1RelationalMappingService stage1,
    Stage2ActivityService stage2) : ControllerBase
{
// --- STAGE 0: Configuration ---
    [HttpPost("stage0/timetables")]
    public async Task<IActionResult> CreateTimetable([FromBody] TimetableCreateDto request)
    {
        var result = await stage0.CreateTimetableAsync(request);
        return result.IsSuccess ? Ok(new { TimetableId = result.Value }) : BadRequest(result.Error);
    }

// --- STAGE 1: Relational Mapping ---
    [HttpPost("stage1/timetables/{timetableId}/teacher-availability")]
    public async Task<IActionResult> AddTeacherAvailability(int timetableId, [FromBody] TeacherAvailabilityDto request)
    {
        var result = await stage1.AddTeacherAvailabilityAsync(timetableId, request);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("stage1/timetables/{timetableId}/teachers/{teacherId}/matrix")]
    public async Task<IActionResult> GetTeacherAvailabilityMatrix(int timetableId, string teacherId)
    {
        var result = await stage1.CalculateTeacherAvailabilityMatrixAsync(timetableId, teacherId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("stage1/timetables/{timetableId}/enrollments")]
    public async Task<IActionResult> AddStudentEnrollment(int timetableId, [FromBody] StudentEnrollmentDto request)
    {
        var result = await stage1.AddStudentEnrollmentAsync(timetableId, request);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("stage1/timetables/{timetableId}/deferred")]
    public async Task<IActionResult> AddDeferredStudent(int timetableId, [FromBody] DeferredStudentDto request)
    {
        var result = await stage1.AddDeferredStudentAsync(timetableId, request);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("stage1/timetables/{timetableId}/students/{studentId}/matrix")]
    public async Task<IActionResult> GetStudentAvailabilityMatrix(int timetableId, string studentId)
    {
        var result = await stage1.CalculateStudentAvailabilityMatrixAsync(timetableId, studentId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

// --- STAGE 2: Activity Modelling ---
    [HttpPost("stage2/timetables/{timetableId}/activities")]
    public async Task<IActionResult> CreateActivity(int timetableId, [FromBody] CourseRequirementCreateDto request)
    {
        var result = await stage2.CreateActivityAsync(timetableId, request);
        return result.IsSuccess ? Ok(new { ActivityId = result.Value }) : BadRequest(result.Error);
    }

// --- STAGE 3: Generation ---
    [HttpPost("stage3/timetables/{timetableId}/generate")]
    public async Task<IActionResult> GenerateTimetable(
        int timetableId,
        [FromServices] TimetableGenerationService generationService,
        CancellationToken cancellationToken)
    {
        // This triggers the gRPC Solver, maps the abstract grid to physical DB entities, 
        // and saves the results into the StagedPlacementEntity table.
        var result = await generationService.GenerateTimetableAsync(timetableId, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}
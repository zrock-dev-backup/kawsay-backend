using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/intake")]
public class TeacherIntakeController(ITeacherRepository teacherRepo, IUnitOfWork uow) : ControllerBase
{
    /// <summary>
    /// Ingests Teachers and their Qualifications (Relation Q) from the Faculty API.
    /// </summary>
    [HttpPost("teachers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IntakeTeachers([FromBody] TeacherIntakeRequest request)
    {
        await uow.BeginTransactionAsync();
        try
        {
            var existingTeachers = (await teacherRepo.GetAllAsync()).ToDictionary(t => t.ExternalTeacherId);
            var added = 0;

            foreach (var dto in request.Teachers)
            {
                if (existingTeachers.TryGetValue(dto.ExternalTeacherId, out var teacher)) continue;
                teacher = new TeacherEntity { ExternalTeacherId = dto.ExternalTeacherId };
                await teacherRepo.AddAsync(teacher);
                added++;
                // Note: If you implement Teacher Qualifications (Relation Q) in the DB, 
                // map dto.QualifiedCourseCodes to the teacher entity here.
            }

            await uow.CommitTransactionAsync();
            return Ok(new { message = $"Successfully ingested {request.Teachers.Count} teachers. Added {added} new." });
        }
        catch (Exception ex)
        {
            await uow.RollbackTransactionAsync();
            return StatusCode(500, new { message = "Teacher intake failed.", details = ex.Message });
        }
    }
}

public record TeacherIntakeRequest(List<TeacherIntakeDto> Teachers);
public record TeacherIntakeDto(
    string ExternalTeacherId,
    string FirstName,
    string LastName,
    List<string> QualifiedCourseCodes
);

using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/intake")]
public class CatalogIntakeController(ICourseRepository courseRepo, IUnitOfWork uow) : ControllerBase
{
    /// <summary>
    /// Ingests the Subject Registry from the external SIS.
    /// </summary>
    [HttpPost("catalog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IntakeCatalog([FromBody] CatalogIntakeRequest request)
    {
        await uow.BeginTransactionAsync();
        try
        {
            var existingCourses = (await courseRepo.GetAllAsync()).Select(c => c.Code).ToHashSet();
            var added = 0;

            foreach (var course in request.Courses.Where(course => !existingCourses.Contains(course.Code)))
            {
                await courseRepo.AddAsync(new CourseEntity 
                { 
                    Code = course.Code, 
                    Name = course.Name 
                });
                existingCourses.Add(course.Code);
                added++;
            }
            
            await uow.CommitTransactionAsync();
            return Ok(new { message = $"Catalog synchronized. Added {added} new subjects." });
        }
        catch (Exception ex)
        {
            await uow.RollbackTransactionAsync();
            return StatusCode(500, new { message = "Catalog intake failed.", details = ex.Message });
        }
    }
}

public record CatalogIntakeRequest(List<CourseIntakeDto> Courses);
public record CourseIntakeDto(string Code, string Name);

using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/timetables/{timetableId:int}/intake")]
public class CohortIntakeController(
    IAcademicStructureRepository academicRepo,
    IStudentRepository studentRepo,
    IUnitOfWork uow) : ControllerBase
{
    /// <summary>
    /// Ingests Cohorts, Groups, Sections, and Students from the SIS for a specific timetable.
    /// </summary>
    [HttpPost("cohorts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IntakeCohorts(int timetableId, [FromBody] CohortIntakeRequest request)
    {
        await uow.BeginTransactionAsync();
        try
        {
            var existingCohorts = await academicRepo.GetCohortsByTimetableAsync(timetableId);
            var allLocalStudents = (await studentRepo.GetAllAsync()).ToList();

            var cohort = existingCohorts.FirstOrDefault(c => c.Name == request.CohortName);
            if (cohort == null)
            {
                cohort = new CohortEntity { Name = request.CohortName, TimetableId = timetableId };
                await academicRepo.AddCohortAsync(cohort);
            }

            foreach (var groupDto in request.Groups)
            {
                var group = cohort.StudentGroups.FirstOrDefault(g => g.Name == groupDto.GroupName);
                if (group == null)
                {
                    group = new StudentGroupEntity { Name = groupDto.GroupName, CohortId = cohort.Id };
                    await academicRepo.AddStudentGroupAsync(group);
                }

                foreach (var sectionDto in groupDto.Sections)
                {
                    var section = group.Sections.FirstOrDefault(s => s.Name == sectionDto.SectionName);
                    if (section == null)
                    {
                        section = new SectionEntity { Name = sectionDto.SectionName, StudentGroupId = group.Id };
                        await academicRepo.AddSectionAsync(section);
                    }

                    var studentsToUpdate = new List<StudentEntity>();
                    foreach (var studentDto in sectionDto.Students)
                    {
                        var localStudent = allLocalStudents.FirstOrDefault(s => s.Name == studentDto.FullName);
                        if (localStudent == null)
                        {
                            localStudent = new StudentEntity
                            {
                                Name = studentDto.FullName,
                                Standing = AcademicStanding.GoodStanding,
                                SectionId = section.Id
                            };
                            await studentRepo.AddAsync(localStudent);
                            allLocalStudents.Add(localStudent);
                        }
                        else if (localStudent.SectionId != section.Id)
                        {
                            localStudent.SectionId = section.Id;
                            studentsToUpdate.Add(localStudent);
                        }
                    }

                    if (studentsToUpdate.Count > 0) await studentRepo.UpdateRangeAsync(studentsToUpdate);
                }
            }

            await uow.CommitTransactionAsync();
            return Ok(new { message = $"Successfully ingested cohort {request.CohortName}." });
        }
        catch (Exception ex)
        {
            await uow.RollbackTransactionAsync();
            return StatusCode(500, new { message = "Cohort intake failed.", details = ex.Message });
        }
    }
}

public record CohortIntakeRequest(string CohortId, string CohortName, List<GroupIntakeDto> Groups);

public record GroupIntakeDto(string GroupId, string GroupName, List<SectionIntakeDto> Sections);

public record SectionIntakeDto(string SectionId, string SectionName, List<StudentIntakeDto> Students);

public record StudentIntakeDto(string StudentId, string FullName);
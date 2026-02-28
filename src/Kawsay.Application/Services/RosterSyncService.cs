using Application.Core;
using Application.DTOs;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class RosterSyncService(
    IAcademicStructureRepository structureRepository,
    ITimetableRepository timetableRepository,
    IStudentRepository studentRepository,
    IAcademicApiClient academicApiClient, // <-- The SIS Telemetry Link
    IUnitOfWork unitOfWork)               // <-- For transactional safety
{
    public async Task<Result<AcademicStructureSyncResultDto>> SyncRosterAsync(int timetableId, string externalCohortId)
    {
        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<AcademicStructureSyncResultDto>.Failure(Error.NotFound("Timetable.NotFound", $"Timetable {timetableId} not found."));

        var startedAt = DateTime.UtcNow;
        var stats = new SyncStats();
        var warnings = new List<string>();

        // 1. Fetch Source Data from SIS
        var sisCohort = await academicApiClient.GetCohortAsync(externalCohortId);
        if (sisCohort == null)
        {
            return Result<AcademicStructureSyncResultDto>.Failure(
                Error.NotFound("SIS.NotFound", $"Cohort '{externalCohortId}' could not be found in the SIS."));
        }

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // 2. Fetch Local State for Upsert Matching
            var existingCohorts = await structureRepository.GetCohortsByTimetableAsync(timetableId);
            var allLocalStudents = (await studentRepository.GetAllAsync()).ToList();

            // --- UPSERT COHORT ---
            // Note: Matching by Name as MVP. Best practice is adding an ExternalId string column to the entities.
            var cohort = existingCohorts.FirstOrDefault(c => c.Name == sisCohort.CohortName);
            if (cohort == null)
            {
                cohort = new CohortEntity { Name = sisCohort.CohortName, TimetableId = timetableId };
                await structureRepository.AddCohortAsync(cohort);
                stats.CohortsCreated++;
            }

            foreach (var sisGroup in sisCohort.Groups)
            {
                // --- UPSERT GROUP ---
                var group = cohort.StudentGroups.FirstOrDefault(g => g.Name == sisGroup.GroupName);
                if (group == null)
                {
                    group = new StudentGroupEntity { Name = sisGroup.GroupName, CohortId = cohort.Id };
                    cohort.StudentGroups.Add(group);
                    await structureRepository.AddStudentGroupAsync(group);
                    stats.GroupsCreated++;
                }

                foreach (var sisLab in sisGroup.Labs)
                {
                    // --- UPSERT SECTION (SIS calls them Labs) ---
                    var section = group.Sections.FirstOrDefault(s => s.Name == sisLab.LabName);
                    if (section == null)
                    {
                        section = new SectionEntity { Name = sisLab.LabName, StudentGroupId = group.Id };
                        group.Sections.Add(section);
                        await structureRepository.AddSectionAsync(section);
                        stats.SectionsCreated++;
                    }

                    // --- UPSERT STUDENTS & ASSIGN TO SECTION ---
                    var studentsToUpdate = new List<StudentEntity>();

                    foreach (var sisStudent in sisLab.Students)
                    {
                        var fullName = $"{sisStudent.FirstName} {sisStudent.LastName}".Trim();
                        
                        // Look for student by name (Again, ExternalId like sisStudent.StudentId is much safer long term)
                        var localStudent = allLocalStudents.FirstOrDefault(s => s.Name == fullName);

                        if (localStudent == null)
                        {
                            // Map SIS Status to Domain Enum
                            var standing = sisStudent.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)
                                ? AcademicStanding.GoodStanding
                                : AcademicStanding.Withdrawn;

                            localStudent = new StudentEntity
                            {
                                Name = fullName,
                                Standing = standing,
                                SectionId = section.Id
                            };
                            
                            await studentRepository.AddAsync(localStudent);
                            allLocalStudents.Add(localStudent); // Track locally to avoid duplicates in same run
                            stats.ProcessedStudents++;
                        }
                        else if (localStudent.SectionId != section.Id)
                        {
                            // Student exists but is moving to a new section
                            localStudent.SectionId = section.Id;
                            studentsToUpdate.Add(localStudent);
                            stats.ProcessedStudents++;
                        }
                    }

                    if (studentsToUpdate.Count > 0)
                    {
                        await studentRepository.UpdateRangeAsync(studentsToUpdate);
                    }
                }
            }

            await unitOfWork.CommitTransactionAsync();

            var message = stats.CohortsCreated == 0 && stats.ProcessedStudents == 0
                ? $"Sync complete. {sisCohort.CohortName} is already up to date."
                : $"Synced {sisCohort.CohortName}. Created {stats.CohortsCreated} cohorts, {stats.GroupsCreated} groups, and processed {stats.ProcessedStudents} students.";

            return Result<AcademicStructureSyncResultDto>.Success(new AcademicStructureSyncResultDto(
                "SIS API",
                startedAt,
                DateTime.UtcNow,
                stats.ProcessedStudents,
                stats.CohortsCreated,
                stats.GroupsCreated,
                stats.SectionsCreated,
                message,
                warnings.Count > 0 ? warnings : null
            ));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result<AcademicStructureSyncResultDto>.Failure(Error.Failure("Sync.Failed", $"Database operation failed: {ex.Message}"));
        }
    }

    private class SyncStats 
    { 
        public int CohortsCreated; 
        public int GroupsCreated; 
        public int SectionsCreated; 
        public int ProcessedStudents; 
    }
}

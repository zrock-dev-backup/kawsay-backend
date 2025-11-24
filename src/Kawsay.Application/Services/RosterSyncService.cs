using Application.Core;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

// TODO: should use external roster service
public class RosterSyncService(
    IAcademicStructureRepository structureRepository,
    ITimetableRepository timetableRepository,
    IStudentRepository studentRepository)
{
    public async Task<Result<AcademicStructureSyncResultDto>> SyncRosterAsync(int timetableId)
    {
        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<AcademicStructureSyncResultDto>.Failure(Error.NotFound("Timetable.NotFound", $"Timetable {timetableId} not found."));

        var startedAt = DateTime.UtcNow;
        var stats = new SyncStats();
        var warnings = new List<string>();

        // 1. Fetch Source Data (Simulated "Mock SIS")
        var sourceData = GetMockSourceData(timetableId);
        if (sourceData == null || sourceData.Count == 0)
        {
             return Result<AcademicStructureSyncResultDto>.Success(new AcademicStructureSyncResultDto(
                "Mock-SIS", startedAt, DateTime.UtcNow, 0, 0, 0, 0, "No source data found for this timetable.", null));
        }

        // 2. Fetch Existing Data to prevent duplicates
        var existingCohorts = await structureRepository.GetCohortsByTimetableAsync(timetableId);

        // 3. Sync Logic
        foreach (var cohortSeed in sourceData)
        {
            var cohort = existingCohorts.FirstOrDefault(c => c.Name == cohortSeed.Name);
            if (cohort == null)
            {
                cohort = new CohortEntity { Name = cohortSeed.Name, TimetableId = timetableId };
                await structureRepository.AddCohortAsync(cohort);
                stats.CohortsCreated++;
            }

            foreach (var groupSeed in cohortSeed.Groups)
            {
                // Re-fetch or navigate to ensure we have latest ID if just created
                // Note: For simplicity in EF, we rely on the object reference 'cohort' which tracks the new ID after AddAsync/Save
                
                var group = cohort.StudentGroups.FirstOrDefault(g => g.Name == groupSeed.Name);
                if (group == null)
                {
                    group = new StudentGroupEntity { Name = groupSeed.Name, CohortId = cohort.Id };
                    // We attach it to the parent for navigation consistency, though AddAsync handles persistence
                    cohort.StudentGroups.Add(group); 
                    await structureRepository.AddStudentGroupAsync(group);
                    stats.GroupsCreated++;
                }

                foreach (var sectionSeed in groupSeed.Sections)
                {
                    var section = group.Sections.FirstOrDefault(s => s.Name == sectionSeed.Name);
                    if (section == null)
                    {
                        section = new SectionEntity { Name = sectionSeed.Name, StudentGroupId = group.Id };
                        group.Sections.Add(section);
                        await structureRepository.AddSectionAsync(section);
                        stats.SectionsCreated++;
                    }

                    // Sync Students into Section
                    var students = await studentRepository.GetByIdsAsync(sectionSeed.StudentIds);
                    foreach (var student in students)
                    {
                        if (student.SectionId != section.Id)
                        {
                            // Only count if we are actually moving/assigning them
                            student.SectionId = section.Id; 
                            stats.ProcessedStudents++;
                        }
                    }
                    await studentRepository.UpdateRangeAsync(students);
                }
            }
        }

        var message = stats.CohortsCreated == 0 && stats.ProcessedStudents == 0
            ? "Roster sync completed. No changes detected."
            : $"Roster sync completed. Created {stats.CohortsCreated} cohorts, {stats.GroupsCreated} groups, and assigned {stats.ProcessedStudents} students.";

        return Result<AcademicStructureSyncResultDto>.Success(new AcademicStructureSyncResultDto(
            "Mock-SIS",
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

    // --- Internal Helper Classes ---
    private class SyncStats { public int CohortsCreated; public int GroupsCreated; public int SectionsCreated; public int ProcessedStudents; }
    private record MockSection(string Name, List<int> StudentIds);
    private record MockGroup(string Name, List<MockSection> Sections);
    private record MockCohort(string Name, List<MockGroup> Groups);

    // --- Data Seeder (Ported from Frontend Mocks) ---
    private List<MockCohort> GetMockSourceData(int timetableId)
    {
        if (timetableId == 1) // Fall 2025
        {
            return new List<MockCohort>
            {
                new("Fall 2025 Intake", new List<MockGroup>
                {
                    new("Fall 2025 - Group A", new List<MockSection>
                    {
                        new("Lab Section A1", [1, 2]), // IDs from Seed Data
                        new("Lab Section A2", [3])
                    }),
                    new("Fall 2025 - Group C", new List<MockSection>
                    {
                        new("Studio Section C1", [4])
                    })
                })
            };
        }
        return new List<MockCohort>();
    }
}

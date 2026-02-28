using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class AcademicStructureService(
    IAcademicStructureRepository structureRepository,
    ITimetableRepository timetableRepository,
    IStudentRepository studentRepository)
{
    public async Task<StudentGroupEntity?> GetStudentGroupByIdAsync(int groupId)
    {
        return await structureRepository.GetStudentGroupByIdAsync(groupId);
    }

    public async Task<SectionEntity?> GetSectionWithStudentsAsync(int sectionId)
    {
        return await structureRepository.GetSectionWithStudentsAsync(sectionId);
    }

    public async Task<CohortDetailDto?> GetCohortDetailsAsync(int cohortId)
    {
        var cohort = await structureRepository.GetCohortByIdAsync(cohortId);
        if (cohort == null) return null;

        return new CohortDetailDto
        {
            Id = cohort.Id,
            Name = cohort.Name,
            TimetableId = cohort.TimetableId,
            StudentGroups = cohort.StudentGroups.Select(g => new StudentGroupDetailDto
            {
                Id = g.Id,
                Name = g.Name,
                Sections = g.Sections.Select(s => new SectionDetailDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Students = new List<StudentDto>()
                }).ToList()
            }).ToList()
        };
    }

    public async Task<List<CohortDetailDto>> GetCohortsByTimetableAsync(int timetableId)
    {
        var cohorts = await structureRepository.GetCohortsByTimetableAsync(timetableId);

        return cohorts.Select(c => new CohortDetailDto
        {
            Id = c.Id,
            Name = c.Name,
            TimetableId = c.TimetableId,
            StudentGroups = c.StudentGroups.Select(g => new StudentGroupDetailDto
            {
                Id = g.Id,
                Name = g.Name,
                Sections = g.Sections.Select(s => new SectionDetailDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Students = []
                }).ToList()
            }).ToList()
        }).ToList();
    }
}
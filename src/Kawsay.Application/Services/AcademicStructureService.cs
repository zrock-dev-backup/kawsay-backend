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

    public async Task<CohortDetailDto?> CreateCohortAsync(CreateCohortRequest request)
    {
        var timetable = await timetableRepository.GetByIdAsync(request.TimetableId);
        if (timetable is null)
        {
            throw new ArgumentException($"Timetable with ID {request.TimetableId} not found.");
        }

        var cohortEntity = new CohortEntity
        {
            Name = request.Name,
            TimetableId = request.TimetableId
        };

        var createdEntity = await structureRepository.AddCohortAsync(cohortEntity);
        return new CohortDetailDto
            { Id = createdEntity.Id, Name = createdEntity.Name, TimetableId = createdEntity.TimetableId };
    }

    public async Task<StudentGroupDetailDto?> CreateStudentGroupAsync(CreateStudentGroupRequest request)
    {
        var cohort = await GetCohortDetailsAsync(request.CohortId);
        if (cohort is null)
        {
            throw new ArgumentException($"Cohort with ID {request.CohortId} not found.");
        }

        var groupEntity = new StudentGroupEntity
        {
            Name = request.Name,
            CohortId = request.CohortId
        };

        var createdEntity = await structureRepository.AddStudentGroupAsync(groupEntity);
        return new StudentGroupDetailDto { Id = createdEntity.Id, Name = createdEntity.Name };
    }

    public async Task<SectionDetailDto?> CreateSectionAsync(CreateSectionRequest request)
    {
        var group = await GetStudentGroupByIdAsync(request.StudentGroupId);
        if (group is null)
        {
            throw new ArgumentException($"Student Group with ID {request.StudentGroupId} not found.");
        }

        var sectionEntity = new SectionEntity
        {
            Name = request.Name,
            StudentGroupId = request.StudentGroupId
        };

        var createdEntity = await structureRepository.AddSectionAsync(sectionEntity);
        return new SectionDetailDto { Id = createdEntity.Id, Name = createdEntity.Name };
    }

    public async Task AssignStudentToSectionAsync(AssignStudentToSectionRequest request)
    {
        var student = await studentRepository.GetByIdAsync(request.StudentId);
        if (student is null)
        {
            throw new ArgumentException($"Student with ID {request.StudentId} not found.");
        }

        var section = await GetSectionWithStudentsAsync(request.SectionId);
        if (section is null)
        {
            throw new ArgumentException($"Section with ID {request.SectionId} not found.");
        }

        await structureRepository.AssignStudentToSectionAsync(student.Id, section.Id);
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
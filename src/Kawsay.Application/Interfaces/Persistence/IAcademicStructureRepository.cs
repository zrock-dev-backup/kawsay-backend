using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IAcademicStructureRepository
{
    // Cohort
    Task<CohortEntity> AddCohortAsync(CohortEntity cohort);
    Task<CohortEntity?> GetCohortByIdAsync(int cohortId);
    Task<List<CohortEntity>> GetCohortsByTimetableAsync(int timetableId);

    // Group
    Task<StudentGroupEntity> AddStudentGroupAsync(StudentGroupEntity studentGroup);
    Task<StudentGroupEntity?> GetStudentGroupByIdAsync(int groupId);

    // Section
    Task<SectionEntity> AddSectionAsync(SectionEntity section);
    Task<SectionEntity?> GetSectionWithStudentsAsync(int sectionId);
    
    // Student Assignment
    Task AssignStudentToSectionAsync(int studentId, int sectionId);
}

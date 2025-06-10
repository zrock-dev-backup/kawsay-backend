using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IStudentModuleGradeRepository
{
    Task AddRangeAsync(IEnumerable<StudentModuleGrade> grades);
    Task<IEnumerable<StudentModuleGrade>> GetGradesByTimetableIdAsync(int timetableId);
}

using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IStudentIssueRepository
{
    Task<List<StudentIssueEntity>> GetActiveIssuesByTimetableAsync(int timetableId);
    Task AddRangeAsync(IEnumerable<StudentIssueEntity> issues);
    Task ResolveIssuesForStudentAsync(int studentId, int timetableId);
}

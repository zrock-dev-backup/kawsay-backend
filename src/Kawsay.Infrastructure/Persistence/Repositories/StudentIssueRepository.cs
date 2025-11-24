using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentIssueRepository(KawsayDbContext context) : IStudentIssueRepository
{
    public async Task<List<StudentIssueEntity>> GetActiveIssuesByTimetableAsync(int timetableId)
    {
        return await context.StudentIssues
            .Where(i => i.TimetableId == timetableId && !i.IsResolved)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<StudentIssueEntity> issues)
    {
        await context.StudentIssues.AddRangeAsync(issues);
        await context.SaveChangesAsync();
    }

    public async Task ResolveIssuesForStudentAsync(int studentId, int timetableId)
    {
        // In a real app, we might just mark IsResolved=true. 
        // For this MVP, we treat "Resolved" as "Deleted" to keep the list clean.
        var issues = await context.StudentIssues
            .Where(i => i.TimetableId == timetableId && i.StudentId == studentId)
            .ToListAsync();
        
        context.StudentIssues.RemoveRange(issues);
        await context.SaveChangesAsync();
    }
}

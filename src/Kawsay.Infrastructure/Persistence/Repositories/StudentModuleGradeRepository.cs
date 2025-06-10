using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentModuleGradeRepository(KawsayDbContext context) : IStudentModuleGradeRepository
{
    public async Task AddRangeAsync(IEnumerable<StudentModuleGrade> grades)
    {
        await context.StudentModuleGrades.AddRangeAsync(grades);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<StudentModuleGrade>> GetGradesByTimetableIdAsync(int timetableId)
    {
        return await context.StudentModuleGrades
            .Where(g => g.TimetableId == timetableId)
            .Include(g => g.Course)
            // We'll include the Student navigation property here once it's defined
            // .Include(g => g.Student) 
            .ToListAsync();
    }
}

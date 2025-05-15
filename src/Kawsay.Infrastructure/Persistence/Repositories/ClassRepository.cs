using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ClassRepository(KawsayDbContext context) : IClassRepository
{
    public async Task<ClassEntity?> GetByIdAsync(int id)
    {
        return await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.ClassOccurrences)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ClassEntity>> GetAllAsync(int timetableId)
    {
        return await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.ClassOccurrences)
            .Include(c => c.PeriodPreferences)
            .Where(c => c.TimetableId == timetableId)
            .ToListAsync();
    }

    public async Task<ClassEntity> AddAsync(ClassEntity lecture)
    {
        context.Classes.Add(lecture);
        await context.SaveChangesAsync();
        // TODO: Why should return lecture?
        return lecture;
    }
}
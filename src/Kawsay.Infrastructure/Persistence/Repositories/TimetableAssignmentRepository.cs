using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TimetableAssignmentRepository(KawsayDbContext context) : ITimetableAssignmentRepository
{
    public async Task<IEnumerable<TimetableAssignmentEntity>> GetByTimetableIdAsync(int timetableId)
    {
        return await context.TimetableAssignments
            .Include(ta => ta.Teacher) // Include Teacher for name resolution
            .Where(ta => ta.TimetableId == timetableId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TimetableAssignmentEntity?> GetByIdAsync(int id)
    {
        return await context.TimetableAssignments.FindAsync(id);
    }

    public async Task<TimetableAssignmentEntity> AddAsync(TimetableAssignmentEntity entity)
    {
        await context.TimetableAssignments.AddAsync(entity);
        return entity;
    }

    public Task DeleteAsync(TimetableAssignmentEntity entity)
    {
        context.TimetableAssignments.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}

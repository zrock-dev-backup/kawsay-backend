using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TimetableRepository(KawsayDbContext context) : ITimetableRepository
{
    public async Task<TimetableEntity?> GetByIdAsync(int id)
    {
        return await context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TimetableEntity>> GetAllAsync()
    {
        return await context.Timetables.ToListAsync();
    }

    public async Task<TimetableEntity> AddAsync(TimetableEntity timetable)
    {
        context.Timetables.Add(timetable);
        await context.SaveChangesAsync();
        // TODO: Why should return lecture?
        return timetable;
    }
}
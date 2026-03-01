using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StagedPlacementRepository(KawsayDbContext context) : IStagedPlacementRepository
{
    public async Task<List<StagedPlacementEntity>> GetByTimetableIdAsync(int timetableId)
    {
        return await context.StagedPlacements
            .Include(sp => sp.CourseRequirement)
            .ThenInclude(cr => cr.Course)
            .Where(sp => sp.CourseRequirement.TimetableId == timetableId)
            .ToListAsync();
    }

    public async Task<StagedPlacementEntity?> GetByIdAsync(int id)
    {
        return await context.StagedPlacements.FindAsync(id);
    }

    public async Task<StagedPlacementEntity> AddAsync(StagedPlacementEntity entity)
    {
        await context.StagedPlacements.AddAsync(entity);
        return entity;
    }

    public Task DeleteAsync(StagedPlacementEntity entity)
    {
        context.StagedPlacements.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAllForTimetableAsync(int timetableId)
    {
        var placements = await context.StagedPlacements
            .Where(sp => sp.CourseRequirement.TimetableId == timetableId)
            .ToListAsync();
        
        if (placements.Count != 0)
        {
            context.StagedPlacements.RemoveRange(placements);
        }
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}

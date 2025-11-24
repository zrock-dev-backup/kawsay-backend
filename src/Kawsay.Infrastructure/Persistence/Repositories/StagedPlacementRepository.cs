using Application.Features.Scheduling.Models;
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

    public async Task<List<StagedPlacement>> GetStagedPlacementsForTimetableAsync(int timetableId)
    {
        var entities = await context.StagedPlacements
            .Include(sp => sp.CourseRequirement)
            .Where(sp => sp.CourseRequirement.TimetableId == timetableId)
            .ToListAsync();

        // Map Entity to Domain/Calculation Model
        return entities.Select(e => new StagedPlacement(
            e.CourseRequirementId,
            e.DayId,
            e.StartPeriodId,
            e.Length,
            // Combine PreferredTeacher and Group into generic "Resources" list for conflict checking
            new List<int> { e.CourseRequirement.PreferredTeacherId ?? 0, e.CourseRequirement.StudentGroupId ?? 0 }
                .Where(id => id != 0)
                .ToList()
        )).ToList();
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

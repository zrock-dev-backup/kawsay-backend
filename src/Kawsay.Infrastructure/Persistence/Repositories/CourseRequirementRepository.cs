using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseRequirementRepository(KawsayDbContext context) : ICourseRequirementRepository
{
    public async Task<CourseRequirementEntity> AddAsync(CourseRequirementEntity requirement)
    {
        await context.CourseRequirements.AddAsync(requirement);
        return requirement; // SaveChangesAsync will be called by the service or UoW
    }

    public async Task<CourseRequirementEntity?> GetByIdAsync(int requirementId)
    {
        return await context.CourseRequirements
            .Include(cr => cr.SoftPreferences)
            .FirstOrDefaultAsync(cr => cr.Id == requirementId);
    }

    public async Task<IEnumerable<CourseRequirementEntity>> GetByTimetableIdAsync(int timetableId)
    {
        return await context.CourseRequirements
            .Where(cr => cr.TimetableId == timetableId)
            .Include(cr => cr.SoftPreferences)
            .ToListAsync();
    }

    public Task UpdateAsync(CourseRequirementEntity requirement)
    {
        context.Entry(requirement).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CourseRequirementEntity requirement)
    {
        context.CourseRequirements.Remove(requirement);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int requirementId)
    {
        return await context.CourseRequirements.AnyAsync(cr => cr.Id == requirementId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}
using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ClassOccurrenceRepository(KawsayDbContext context) : IClassOccurrenceRepository
{
    public async Task AddRangeAsync(List<ClassOccurrenceEntity> classOccurrences)
    {
        await context.AddRangeAsync(classOccurrences);
        await context.SaveChangesAsync();
    }

    public async Task DeleteByClassIdAsync(List<int> classIds)
    {
        if (classIds.Count == 0)
        {
            return;
        }

        var occurrencesToDelete = await context.ClassOccurrences
            .Where(co => classIds.Contains(co.ClassId))
            .ToListAsync();

        if (occurrencesToDelete.Count != 0)
        {
            context.ClassOccurrences.RemoveRange(occurrencesToDelete);
            await context.SaveChangesAsync();
        }
    }
}
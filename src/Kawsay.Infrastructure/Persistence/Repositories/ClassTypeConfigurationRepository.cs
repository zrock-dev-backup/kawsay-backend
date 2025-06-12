using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ClassTypeConfigurationRepository(KawsayDbContext context) : IClassTypeConfigurationRepository
{
    public async Task<ClassTypeConfigurationEntity?> GetByTypeAsync(Domain.Enums.ClassType classType)
    {
        return await context.ClassTypeConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClassType == classType);
    }

    public async Task<List<ClassTypeConfigurationEntity>> GetAllAsync()
    {
        return await context.ClassTypeConfigurations.AsNoTracking().ToListAsync();
    }

    public async Task UpsertRangeAsync(IEnumerable<ClassTypeConfigurationEntity> configurations)
    {
        foreach (var config in configurations)
        {
            var existing = await context.ClassTypeConfigurations
                .FirstOrDefaultAsync(c => c.ClassType == config.ClassType);

            if (existing != null)
            {
                existing.DefaultLength = config.DefaultLength;
            }
            else
            {
                await context.ClassTypeConfigurations.AddAsync(config);
            }
        }

        await context.SaveChangesAsync();
    }
}
// src/Kawsay.Infrastructure/Persistence/Repositories/FacultyAvailabilityRepository.cs
using Application.Interfaces.Persistence;
using Domain.Entities.availability;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FacultyAvailabilityRepository(KawsayDbContext context) : IFacultyAvailabilityRepository
{
    // Note: We use "faculty" as the entity_type discriminator to ensure this repository 
    // only ever returns or interacts with faculty availability records.
    private const string Discriminator = "faculty";

    public async Task AddRangeAsync(IEnumerable<AvailabilityEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.EntityType = Discriminator; // Enforce entity type
        }
        await context.Availabilities.AddRangeAsync(entities);
    }

    public async Task<List<AvailabilityEntity>> GetByFacultyIdAsync(int facultyId)
    {
        return await context.Availabilities
            .Where(a => a.EntityType == Discriminator && a.EntityId == facultyId)
            .ToListAsync();
    }

    public async Task<List<AvailabilityEntity>> GetAllAsync()
    {
        return await context.Availabilities
            .Where(a => a.EntityType == Discriminator)
            .ToListAsync();
    }

    public async Task<AvailabilityEntity?> GetByIdAsync(int id)
    {
        return await context.Availabilities
            .FirstOrDefaultAsync(a => a.Id == id && a.EntityType == Discriminator);
    }

    public Task DeleteAsync(AvailabilityEntity entity)
    {
        context.Availabilities.Remove(entity);
        return Task.CompletedTask; // SaveChangesAsync is handled by the UnitOfWork
    }
}

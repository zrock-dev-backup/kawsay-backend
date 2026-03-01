using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IStagedPlacementRepository
{
    // For Service logic (Entity based)
    Task<List<StagedPlacementEntity>> GetByTimetableIdAsync(int timetableId);
    
    Task<StagedPlacementEntity?> GetByIdAsync(int id);
    Task<StagedPlacementEntity> AddAsync(StagedPlacementEntity entity);
    Task DeleteAsync(StagedPlacementEntity entity);
    Task DeleteAllForTimetableAsync(int timetableId);
    Task SaveChangesAsync();
}

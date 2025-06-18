using Application.Features.Scheduling.Models;

namespace Application.Interfaces.Persistence
{
    public interface IStagedPlacementRepository
    {
        Task<List<StagedPlacement>> GetStagedPlacementsForTimetableAsync(int timetableId);
    }
}

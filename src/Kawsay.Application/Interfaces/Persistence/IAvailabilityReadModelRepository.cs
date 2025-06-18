using Application.Features.Scheduling.Models;

namespace Application.Interfaces.Persistence
{
    public interface IAvailabilityReadModelRepository
    {
        Task<Dictionary<int, SchedulingMatrix>> GetMatricesForResourcesAsync(List<int> resourceIds);
    }
}
namespace Application.Features.Scheduling.Models
{
    public record StagedPlacement(int RequirementId, int DayId, int StartPeriodId, int Length, List<int> ResourceIds);
}
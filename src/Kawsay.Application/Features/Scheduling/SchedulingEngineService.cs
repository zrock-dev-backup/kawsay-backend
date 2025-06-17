using Application.Features.Scheduling.Models;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Features.Scheduling;

public class SchedulingEngineService
{
    private readonly ICourseRequirementRepository _requirementRepo;
    private readonly IAvailabilityReadModelRepository _availabilityRepo;
    private readonly IStagedPlacementRepository _stagedPlacementRepo;

    public SchedulingEngineService(
        ICourseRequirementRepository requirementRepo,
        IAvailabilityReadModelRepository availabilityRepo,
        IStagedPlacementRepository stagedPlacementRepo)
    {
        _requirementRepo = requirementRepo;
        _availabilityRepo = availabilityRepo;
        _stagedPlacementRepo = stagedPlacementRepo;
    }

    public async Task<List<ValidSlotDto>> GetValidSlotsForRequirementAsync(int requirementId)
    {
        // 1. DATA GATHERING
        var requirement = await _requirementRepo.GetByIdWithDetailsAsync(requirementId)
                          ?? throw new KeyNotFoundException(
                              $"CourseRequirement with ID {requirementId} not found.");

        var resourceIds = GetResourceIdsForRequirement(requirement);
        var baseMatrices = await _availabilityRepo.GetMatricesForResourcesAsync(resourceIds);
        var stagedPlacements =
            await _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(requirement.Timetable.Id);

        // 2. MATRIX COMBINATION (Apply hard constraints)
        var combinedMatrix = CombineAvailabilityMatrices(baseMatrices, requirement.Timetable);
        ApplyStagedPlacementsAsConflicts(combinedMatrix, stagedPlacements, resourceIds, requirement.Timetable);

        // 3. FILTERING AND SCORING PIPELINE
        var validSlots = new List<ValidSlotDto>();
        var dayMap = requirement.Timetable.Days.ToDictionary(d => d.Id);
        var periodMap = requirement.Timetable.Periods.ToDictionary(p => p.Id);

        // Create index maps for matrix operations
        var dayIdToIndex = requirement.Timetable.Days.Select((d, i) => new { d.Id, Index = i })
            .ToDictionary(x => x.Id, x => x.Index);
        var periodIdToIndex = requirement.Timetable.Periods.Select((p, i) => new { p.Id, Index = i })
            .ToDictionary(x => x.Id, x => x.Index);

        foreach (var day in requirement.Timetable.Days)
        {
            foreach (var period in requirement.Timetable.Periods)
            {
                if (IsSlotValid(day.Id, period.Id, requirement, combinedMatrix, dayIdToIndex, periodIdToIndex))
                {
                    var slotType = IsSlotPreferred(day.Id, period.Id, requirement)
                        ? SlotType.Ideal
                        : SlotType.Viable;
                    var guidanceScore = CalculateGuidanceScore(day.Id, period.Id, requirement, baseMatrices,
                        slotType, dayIdToIndex, periodIdToIndex);

                    validSlots.Add(new ValidSlotDto
                    {
                        DayId = day.Id,
                        StartPeriodId = period.Id,
                        Type = slotType,
                        GuidanceScore = guidanceScore
                    });
                }
            }
        }

        return validSlots;
    }

    private List<int> GetResourceIdsForRequirement(CourseRequirementEntity requirement)
    {
        var resourceIds = new List<int>();
        if (requirement.TeacherId.HasValue)
        {
            resourceIds.Add(requirement.TeacherId.Value);
        }

        if (requirement.StudentGroupId.HasValue)
        {
            resourceIds.Add(requirement.StudentGroupId.Value);
        }

        return resourceIds;
    }

    private SchedulingMatrix CombineAvailabilityMatrices(Dictionary<int, SchedulingMatrix> resourceMatrices,
        TimetableEntity timetable)
    {
        var combined = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
        foreach (var matrix in resourceMatrices.Values)
        {
            for (int r = 0; r < combined.Rows; r++)
            {
                for (int c = 0; c < combined.Columns; c++)
                {
                    if (matrix.Get(r, c) == 1)
                    {
                        combined.Set(r, c, 1);
                    }
                }
            }
        }

        return combined;
    }

    private void ApplyStagedPlacementsAsConflicts(SchedulingMatrix matrix, List<StagedPlacement> stagedPlacements,
        List<int> requiredResourceIds, TimetableEntity timetable)
    {
        var dayIdToIndex = timetable.Days.Select((d, i) => new { d.Id, Index = i })
            .ToDictionary(x => x.Id, x => x.Index);
        var periodIdToIndex = timetable.Periods.Select((p, i) => new { p.Id, Index = i })
            .ToDictionary(x => x.Id, x => x.Index);

        foreach (var placement in stagedPlacements)
        {
            // If the staged placement involves any of the resources we need, it's a conflict.
            if (placement.ResourceIds.Any(id => requiredResourceIds.Contains(id)))
            {
                int dayIndex = dayIdToIndex[placement.DayId];
                int startPeriodIndex = periodIdToIndex[placement.StartPeriodId];

                for (int i = 0; i < placement.Length; i++)
                {
                    matrix.Set(dayIndex, startPeriodIndex + i, 1); // Mark as busy
                }
            }
        }
    }

    private bool IsSlotValid(int dayId, int startPeriodId, CourseRequirementEntity requirement,
        SchedulingMatrix combinedMatrix, Dictionary<int, int> dayMap, Dictionary<int, int> periodMap)
    {
        if (!dayMap.ContainsKey(dayId) || !periodMap.ContainsKey(startPeriodId)) return false;

        int dayIndex = dayMap[dayId];
        int startPeriodIndex = periodMap[startPeriodId];

        if (startPeriodIndex + requirement.Length > combinedMatrix.Columns) return false; // Doesn't fit in the day

        for (int i = 0; i < requirement.Length; i++)
        {
            if (combinedMatrix.Get(dayIndex, startPeriodIndex + i) == 1)
            {
                return false; // Conflict found
            }
        }

        return true;
    }

    private bool IsSlotPreferred(int dayId, int startPeriodId, CourseRequirementEntity requirement)
    {
        return requirement.PeriodPreferences.Any(p => p.DayId == dayId && p.StartPeriodId == startPeriodId);
    }

    private double CalculateGuidanceScore(int dayId, int startPeriodId, CourseRequirementEntity requirement,
        Dictionary<int, SchedulingMatrix> resourceMatrices, SlotType slotType, Dictionary<int, int> dayMap,
        Dictionary<int, int> periodMap)
    {
        double score = (slotType == SlotType.Ideal) ? 100.0 : 50.0;

        int dayIndex = dayMap[dayId];
        int startPeriodIndex = periodMap[startPeriodId];
        int endPeriodIndex = startPeriodIndex + requirement.Length - 1;

        foreach (var resourceId in resourceMatrices.Keys)
        {
            var matrix = resourceMatrices[resourceId];

            // Heuristic 1: Contiguity Bonus (reward slots next to existing events)
            if (matrix.Get(dayIndex, startPeriodIndex - 1) == 1) score += 10.0;
            if (matrix.Get(dayIndex, endPeriodIndex + 1) == 1) score += 10.0;

            // Heuristic 2: Fragmentation Penalty (penalize creating small, unusable free blocks)
            // Check the block of free time BEFORE this placement
            if (matrix.Get(dayIndex, startPeriodIndex - 1) == 0) // Is there a free block before?
            {
                int freeBlockSize =
                    GetAdjacentFreeBlockSize(matrix, dayIndex, startPeriodIndex - 1, lookLeft: true);
                if (freeBlockSize > 0 && freeBlockSize < requirement.Length)
                {
                    score -= 25.0; // Penalize for leaving a small fragment
                }
            }

            // Check the block of free time AFTER this placement
            if (matrix.Get(dayIndex, endPeriodIndex + 1) == 0) // Is there a free block after?
            {
                int freeBlockSize = GetAdjacentFreeBlockSize(matrix, dayIndex, endPeriodIndex + 1, lookLeft: false);
                if (freeBlockSize > 0 && freeBlockSize < requirement.Length)
                {
                    score -= 25.0; // Penalize for leaving a small fragment
                }
            }
        }

        return score;
    }

    private int GetAdjacentFreeBlockSize(SchedulingMatrix matrix, int dayIndex, int startPeriodIndex, bool lookLeft)
    {
        int size = 0;
        int currentPeriod = startPeriodIndex;
        int increment = lookLeft ? -1 : 1;

        while (matrix.Get(dayIndex, currentPeriod) == 0)
        {
            size++;
            currentPeriod += increment;
        }

        return size;
    }
}

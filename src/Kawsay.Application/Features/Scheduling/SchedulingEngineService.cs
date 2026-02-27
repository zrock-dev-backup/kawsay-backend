using Application.Features.Scheduling.Models;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Features.Scheduling
{
    // Deprecated, use GRPc implementation
    // Marked for deletion. Along overall implementation
    public class SchedulingEngineService(
        ICourseRequirementRepository requirementRepo,
        IAvailabilityReadModelRepository availabilityRepo,
        IStagedPlacementRepository stagedPlacementRepo)
    {
        public async Task<List<ValidSlotDto>> GetValidSlotsForRequirementAsync(int requirementId)
        {
            var requirement = await requirementRepo.GetByIdAsync(requirementId)
                              ?? throw new KeyNotFoundException(
                                  $"CourseRequirement with ID {requirementId} not found.");

            var resourceIds = GetResourceIdsForRequirement(requirement);
            var baseMatrices = await availabilityRepo.GetMatricesForResourcesAsync(resourceIds);
            var stagedPlacements =
                await stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(requirement.Timetable.Id);

            var combinedMatrix = CombineAvailabilityMatrices(baseMatrices, requirement.Timetable);
            ApplyStagedPlacementsAsConflicts(combinedMatrix, stagedPlacements, resourceIds, requirement.Timetable);

            var validSlots = new List<ValidSlotDto>();
            var dayIdToIndex = requirement.Timetable.Days.Select((d, i) => new { d.Id, Index = i })
                .ToDictionary(x => x.Id, x => x.Index);
            var periodIndexToId = requirement.Timetable.Periods.Select((p, i) => new { p.Id, Index = i })
                .ToDictionary(x => x.Index, x => x.Id);
            var periodIdToIndex = requirement.Timetable.Periods.Select((p, i) => new { p.Id, Index = i })
                .ToDictionary(x => x.Id, x => x.Index);

            foreach (var day in requirement.Timetable.Days)
            {
                int dayIndex = dayIdToIndex[day.Id];
                var validStarts = FindValidStartingIndices(combinedMatrix, dayIndex, requirement.DurationInPeriods,
                    requirement.Timetable.Periods.Count);

                foreach (var startIndex in validStarts)
                {
                    int startPeriodId = periodIndexToId[startIndex];
                    var slotType = SlotType.Viable;
                    var guidanceScore = CalculateGuidanceScore(day.Id, startPeriodId, requirement, baseMatrices,
                        slotType, dayIdToIndex, periodIdToIndex);
                    validSlots.Add(new ValidSlotDto
                    {
                        DayId = day.Id,
                        StartPeriodId = startPeriodId,
                        Type = slotType,
                        GuidanceScore = guidanceScore
                    });
                }
            }

            return validSlots;
        }

        private List<int> GetResourceIdsForRequirement(CourseRequirementEntity requirement)
        {
            var resourceIds = new List<int>();
            if (requirement.PreferredTeacherId.HasValue)
            {
                resourceIds.Add(requirement.PreferredTeacherId.Value);
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
            if (resourceMatrices.Count == 0)
            {
                return new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
            }

            var firstMatrix = resourceMatrices.Values.First();
            var combined = new SchedulingMatrix(firstMatrix.Rows, firstMatrix.Columns);

            for (int r = 0; r < combined.Rows; r++)
            {
                for (int c = 0; c < combined.Columns; c++)
                {
                    bool isBusy = resourceMatrices.Values.Any(m => m.Get(r, c) == 1);
                    combined.Set(r, c, isBusy ? 1 : 0);
                }
            }

            return combined;
        }

        private void ApplyStagedPlacementsAsConflicts(SchedulingMatrix matrix, List<StagedPlacement> stagedPlacements,
            List<int> requiredResourceIds, TimetableEntity timetable)
        {
            var dayIdToIndex = timetable.Days
                .Select((d, index) => new { d.Id, index })
                .ToDictionary(x => x.Id, x => x.index);

            var periodIdToIndex = timetable.Periods
                .Select((p, index) => new { p.Id, index })
                .ToDictionary(x => x.Id, x => x.index);

            foreach (var placement in stagedPlacements)
            {
                if (placement.ResourceIds.Any(id => requiredResourceIds.Contains(id)))
                {
                    int dayIndex = dayIdToIndex[placement.DayId];
                    int startPeriodIndex = periodIdToIndex[placement.StartPeriodId];
                    for (int i = 0; i < placement.Length; i++)
                    {
                        matrix.Set(dayIndex, startPeriodIndex + i, 1);
                    }
                }
            }
        }

        private List<int> FindValidStartingIndices(SchedulingMatrix matrix, int dayIndex, int length, int periodCount)
        {
            var validStarts = new List<int>();
            if (length > periodCount) return validStarts;

            int windowSum = 0;
            for (int i = 0; i < length; i++)
            {
                windowSum += matrix.Get(dayIndex, i);
            }

            if (windowSum == 0)
            {
                validStarts.Add(0);
            }

            for (int j = 1; j <= periodCount - length; j++)
            {
                windowSum -= matrix.Get(dayIndex, j - 1);
                windowSum += matrix.Get(dayIndex, j + length - 1);
                if (windowSum == 0)
                {
                    validStarts.Add(j);
                }
            }

            return validStarts;
        }

        private double CalculateGuidanceScore(int dayId, int startPeriodId, CourseRequirementEntity requirement,
            Dictionary<int, SchedulingMatrix> resourceMatrices, SlotType slotType, Dictionary<int, int> dayIdToIndex,
            Dictionary<int, int> periodIdToIndex)
        {
            double score = (slotType == SlotType.Ideal) ? 100.0 : 50.0;

            int dayIndex = dayIdToIndex[dayId];
            int startPeriodIndex = periodIdToIndex[startPeriodId];
            int endPeriodIndex = startPeriodIndex + requirement.DurationInPeriods - 1;

            foreach (var resourceId in resourceMatrices.Keys)
            {
                var matrix = resourceMatrices[resourceId];

                if (startPeriodIndex > 0 && matrix.Get(dayIndex, startPeriodIndex - 1) == 1)
                    score += 10.0;
                if (endPeriodIndex < matrix.Columns - 1 && matrix.Get(dayIndex, endPeriodIndex + 1) == 1)
                    score += 10.0;

                if (startPeriodIndex > 0 && matrix.Get(dayIndex, startPeriodIndex - 1) == 0)
                {
                    int freeBlockSize = GetAdjacentFreeBlockSize(matrix, dayIndex, startPeriodIndex - 1, true);
                    if (freeBlockSize > 0 && freeBlockSize < requirement.DurationInPeriods)
                        score -= 25.0;
                }

                if (endPeriodIndex < matrix.Columns - 1 && matrix.Get(dayIndex, endPeriodIndex + 1) == 0)
                {
                    int freeBlockSize = GetAdjacentFreeBlockSize(matrix, dayIndex, endPeriodIndex + 1, false);
                    if (freeBlockSize > 0 && freeBlockSize < requirement.DurationInPeriods)
                        score -= 25.0;
                }
            }

            return score;
        }

        private int GetAdjacentFreeBlockSize(SchedulingMatrix matrix, int dayIndex, int startPeriodIndex, bool lookLeft)
        {
            int size = 0;
            int currentPeriod = startPeriodIndex;
            int increment = lookLeft ? -1 : 1;
            while (currentPeriod >= 0 && currentPeriod < matrix.Columns && matrix.Get(dayIndex, currentPeriod) == 0)
            {
                size++;
                currentPeriod += increment;
            }

            return size;
        }
    }
}
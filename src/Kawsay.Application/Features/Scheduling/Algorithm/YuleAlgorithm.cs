using Application.Features.Scheduling.Models;

namespace Application.Features.Scheduling.Algorithm;

public static class YuleAlgorithm
{
    public static bool Handler(SchedulingRequirementLine requirementLine, Dictionary<int, SchedulingEntity> entities,
        int day,
        int period)
    {
        requirementLine.AvailabilityMatrix = new SchedulingMatrix(day, period);
        for (var i = 0; i < requirementLine.Frequency; i++)
        {
            PopulateLineAvailabilityMatrix(requirementLine, entities, day, period);
            if (Schedule(requirementLine, entities, day, period)) continue;
            Console.WriteLine(
                $"Failed to schedule occurrence {i + 1}/{requirementLine.Frequency} for requirement S=[{string.Join(",", requirementLine.EntitiesList)}] (q={requirementLine.Frequency}, len={requirementLine.Length}).");

            return false;
        }

        return true;
    }

    private static void PopulateLineAvailabilityMatrix(SchedulingRequirementLine requirementLine,
        Dictionary<int, SchedulingEntity> entities,
        int day, int period)
    {
        var lineAvailabilityMatrix = requirementLine.AvailabilityMatrix;
        for (var dayIndex = 0; dayIndex < day; dayIndex++)
        for (var periodIndex = 0; periodIndex < period; periodIndex++)
        {
            var allRequiredEntitiesAvailable = requirementLine.EntitiesList.All(entityId =>
            {
                if (entities.TryGetValue(entityId, out var entity))
                {
                    return entity.AvailabilityMatrix.Get(dayIndex, periodIndex) == 0;
                }

                Console.WriteLine(
                    $"Error: Required entity ID {entityId} not found in global entities list during E matrix population for requirement S=[{string.Join(",", requirementLine.EntitiesList)}]. Treating slot [{dayIndex},{periodIndex}] as unavailable.");
                return false;
            });

            var isPreferredSlot = requirementLine.PeriodPreferenceMatrix.Get(dayIndex, periodIndex) == 0;
            var isAvailable = allRequiredEntitiesAvailable && isPreferredSlot;
            lineAvailabilityMatrix.Set(dayIndex, periodIndex, isAvailable ? 0 : 1);
        }
    }

    private static bool Schedule(SchedulingRequirementLine requirementLine, Dictionary<int, SchedulingEntity> entities,
        int day, int period)
    {
        var candidateSlots = new List<TimetablePair>();
        for (var dayIndex = 0; dayIndex < day; dayIndex++)
        for (var periodIndex = 0; periodIndex < period; periodIndex++)
        {
            if (requirementLine.AvailabilityMatrix.Get(dayIndex, periodIndex) == 0)
            {
                if (ValidateLineRequirementAvailability(dayIndex, periodIndex, requirementLine, entities))
                {
                    candidateSlots.Add(new TimetablePair(dayIndex, periodIndex));
                }
            }
        }

        if (candidateSlots.Count == 0) return false;

        TimetablePair? bestSlot = null;
        var bestScore = double.MinValue;
        foreach (var candidate in candidateSlots)
        {
            var score = ScoreCandidateSlot(candidate, requirementLine, entities);
            if (score > bestScore)
            {
                bestScore = score;
                bestSlot = candidate;
            }
        }

        UpdateEntitiesAvailability(bestSlot!.Day, bestSlot.Period, requirementLine, entities);
        requirementLine.AssignedTimeslotList.Add(bestSlot);
        return true;
    }

    private static double ScoreCandidateSlot(TimetablePair candidate, SchedulingRequirementLine requirement,
        Dictionary<int, SchedulingEntity> allEntities)
    {
        var score = 100.0;

        if (requirement.Frequency > 1)
        {
            var dayAlreadyHasOccurrence = requirement.AssignedTimeslotList.Any(slot => slot.Day == candidate.Day);
            if (!dayAlreadyHasOccurrence)
            {
                score += 50.0;
            }
        }

        foreach (var entityId in requirement.EntitiesList)
        {
            if (!allEntities.TryGetValue(entityId, out var entity)) continue;

            var entityMatrix = entity.AvailabilityMatrix;
            var candidateStart = candidate.Period;
            var candidateEnd = candidate.Period + requirement.Length - 1;

            // Heuristic 2: Contiguity Bonus
            // Reward slots that are immediately before or after an existing class for this entity.
            if (entityMatrix.Get(candidate.Day, candidateStart - 1) == 1) // Slot immediately before is busy
            {
                score += 10.0;
            }

            if (entityMatrix.Get(candidate.Day, candidateEnd + 1) == 1) // Slot immediately after is busy
            {
                score += 10.0;
            }

            // Heuristic 3: Gap Penalty
            // Penalize slots that create a single-period "hole" in the entity's schedule.
            var isGapBefore = entityMatrix.Get(candidate.Day, candidateStart - 1) == 0 && // Slot before is free
                              entityMatrix.Get(candidate.Day, candidateStart - 2) == 1; // Slot 2-before is busy
            if (isGapBefore)
            {
                score -= 25.0;
            }

            var isGapAfter = entityMatrix.Get(candidate.Day, candidateEnd + 1) == 0 && // Slot after is free
                             entityMatrix.Get(candidate.Day, candidateEnd + 2) == 1; // Slot 2-after is busy
            if (isGapAfter)
            {
                score -= 25.0;
            }
        }

        return score;
    }

    private static bool ValidateLineRequirementAvailability(int dayIndex, int startPeriodIndex,
        SchedulingRequirementLine requirementLine, Dictionary<int, SchedulingEntity> entities)
    {
        var totalTimetablePeriodAmt = requirementLine.AvailabilityMatrix.Columns;
        var remainingPeriods = totalTimetablePeriodAmt - startPeriodIndex;
        if (remainingPeriods < requirementLine.Length) return false;
        for (var k = 0; k < requirementLine.Length; k++)
        {
            var currentPeriodIndex = startPeriodIndex + k;
            foreach (var entityId in requirementLine.EntitiesList)
            {
                if (!entities.TryGetValue(entityId, out var entity))
                {
                    Console.WriteLine(
                        $"Error: Required entity ID {entityId} not found in the global entities list during validation.");
                    return false;
                }

                if (entity.AvailabilityMatrix.Get(dayIndex, currentPeriodIndex) == 1) return false;
            }
        }

        return true;
    }


    private static void UpdateEntitiesAvailability(int dayIndex, int startPeriodIndex,
        SchedulingRequirementLine requirementLine, Dictionary<int, SchedulingEntity> entities)
    {
        for (var k = 0; k < requirementLine.Length; k++)
        {
            var currentPeriodIndex = startPeriodIndex + k;
            foreach (var entityId in requirementLine.EntitiesList)
            {
                if (entities.TryGetValue(entityId, out var entity))
                {
                    entity.AvailabilityMatrix.Set(dayIndex, currentPeriodIndex, 1);
                }
                else
                {
                    Console.WriteLine(
                        $"Error: Required entity ID {entityId} not found in the global entities list during jC update.");
                }
            }
        }
    }
}
using Application.Features.Scheduling.Models;

namespace Application.Features.Scheduling.Utils;

public static class SchedulingHeuristics
{
    public static bool IsSlotConflictFree(int dayIndex, int startPeriodIndex, int length,
        SchedulingMatrix combinedMatrix)
    {
        // Boundary check to prevent running off the end of the timetable
        if (startPeriodIndex + length > combinedMatrix.Columns)
        {
            return false;
        }

        for (var i = 0; i < length; i++)
        {
            if (combinedMatrix.Get(dayIndex, startPeriodIndex + i) == 1)
            {
                // Conflict found, this slot is not viable.
                return false;
            }
        }

        return true; // No conflicts found.
    }

    public static double CalculateSuitabilityScore(
        int dayIndex,
        int startPeriodIndex,
        EffectiveRequirementParameters parameters,
        IReadOnlyDictionary<int, SchedulingMatrix> resourceMatrices)
    {
        // Base score for any viable slot.
        var score = 50.0;
        var endPeriodIndex = startPeriodIndex + parameters.Length - 1;

        // Analyze the impact on each resource's schedule individually.
        foreach (var matrix in resourceMatrices.Values)
        {
            // Heuristic 1: Contiguity Bonus
            // Reward slots that are immediately adjacent to existing busy blocks.
            if (matrix.Get(dayIndex, startPeriodIndex - 1) == 1) score += 10.0; // Slot before is busy
            if (matrix.Get(dayIndex, endPeriodIndex + 1) == 1) score += 10.0; // Slot after is busy

            // Heuristic 2: Gap Penalty
            // Penalizes slots that create a single-period "hole" in a schedule.
            var createsGapBefore = matrix.Get(dayIndex, startPeriodIndex - 1) == 0 && // Slot before is free
                                   matrix.Get(dayIndex, startPeriodIndex - 2) == 1; // Slot 2-before is busy
            if (createsGapBefore) score -= 25.0;

            var createsGapAfter = matrix.Get(dayIndex, endPeriodIndex + 1) == 0 && // Slot after is free
                                  matrix.Get(dayIndex, endPeriodIndex + 2) == 1; // Slot 2-after is busy
            if (createsGapAfter) score -= 25.0;
        }

        // Clamp the score to a predictable 0-100 range.
        return Math.Max(0, Math.Min(100, score));
    }
}
using Application.Features.Scheduling.Models;

namespace Application.Features.Scheduling.Algorithm;

public static class YuleAlgorithm
{
    public static bool Handler(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int day,
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


    private static void PopulateLineAvailabilityMatrix(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities,
        int day, int period)
    {
        var lineAvailabilityMatrix = requirementLine.AvailabilityMatrix;
        for (var dayIndex = 0; dayIndex < day; dayIndex++)
        for (var periodIndex = 0; periodIndex < period; periodIndex++)
        {
            // Tt filter has been removed
            // verification of all entities availability first member of formula 1
            var allRequiredEntitiesAvailable = requirementLine.EntitiesList.All(entityId =>
            {
                var entity = entities.FirstOrDefault(e => e.Id == entityId);
                if (entity != null) return entity.AvailabilityMatrix.Get(dayIndex, periodIndex) == 0;
                Console.WriteLine(
                    $"Error: Required entity ID {entityId} not found in global entities list during E matrix population for requirement S=[{string.Join(",", requirementLine.EntitiesList)}]. Treating slot [{dayIndex},{periodIndex}] as unavailable.");
                return false;

            });
            if (!allRequiredEntitiesAvailable)
            {
                lineAvailabilityMatrix.Set(dayIndex, periodIndex, 1);
                continue;
            }
            
            var constraint = requirementLine.PeriodPreferenceList[periodIndex];
            lineAvailabilityMatrix.Set(dayIndex, periodIndex, constraint == 0 ? 0 : 1);
        }
    }


    private static bool Schedule(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities,
        int day, int period)
    {
        var availabilityMatrix = requirementLine.AvailabilityMatrix;
        for (var dayIndex = 0; dayIndex < day; dayIndex++)
        for (var periodIndex = 0; periodIndex < period; periodIndex++)
            if (availabilityMatrix.Get(dayIndex, periodIndex) == 0)
                if (ValidateLineRequirementAvailability(dayIndex, periodIndex, requirementLine, entities))
                {
                    UpdateEntitiesAvailability(dayIndex, periodIndex, requirementLine, entities);
                    requirementLine.AssignedTimeslotList.Add(new TimetablePair(dayIndex, periodIndex));
                    return true;
                }
        return false;
    }


    private static bool ValidateLineRequirementAvailability(int dayIndex, int startPeriodIndex,
        SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities)
    {
        var totalTimetablePeriodAmt = requirementLine.AvailabilityMatrix.Columns;
        var remainingPeriods = totalTimetablePeriodAmt - startPeriodIndex;
        if (remainingPeriods < requirementLine.Length) return false;
        for (var k = 0; k < requirementLine.Length; k++)
        {
            var currentPeriodIndex = startPeriodIndex + k;
            foreach (var entityId in requirementLine.EntitiesList)
            {
                var entity = entities.FirstOrDefault(e => e.Id == entityId);
                if (entity == null)
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
        SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities)
    {
        for (var k = 0; k < requirementLine.Length; k++)
        {
            var currentPeriodIndex = startPeriodIndex + k;
            foreach (var entityId in requirementLine.EntitiesList)
            {
                var entity = entities.FirstOrDefault(e => e.Id == entityId);
                if (entity == null)
                {
                    Console.WriteLine(
                        $"Error: Required entity ID {entityId} not found in the global entities list during jC update.");
                    continue;
                }
                entity.AvailabilityMatrix.Set(dayIndex, currentPeriodIndex, 1);
            }
        }
    }
}
namespace kawsay.Scheduling;

public static class SchedulingAlgorithm
{
    public static readonly List<string> dayOrder = new()
        { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };


    public static bool Handler(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays,
        int numPeriods)
    {
        requirementLine.E = new SchedulingMatrix(numDays, numPeriods);


        for (var i = 0; i < requirementLine.q; i++)
        {
            PopulateEMatrix(requirementLine, entities, numDays, numPeriods);


            if (!Schedule(requirementLine, entities, numDays, numPeriods))
            {
                Console.WriteLine(
                    $"Failed to schedule occurrence {i + 1}/{requirementLine.q} for requirement S=[{string.Join(",", requirementLine.S)}] (q={requirementLine.q}, len={requirementLine.length}).");

                return false;
            }
        }


        return true;
    }


    private static void PopulateEMatrix(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities,
        int numDays, int numPeriods)
    {
        var matrixE = requirementLine.E;


        for (var dayIndex = 0; dayIndex < numDays; dayIndex++)
        for (var periodIndex = 0; periodIndex < numPeriods; periodIndex++)
        {
            var allRequiredEntitiesAvailable = requirementLine.S.All(requiredEntityId =>
            {
                var entity = entities.FirstOrDefault(e => e.Id == requiredEntityId);
                if (entity == null)
                {
                    Console.WriteLine(
                        $"Error: Required entity ID {requiredEntityId} not found in global entities list during E matrix population for requirement S=[{string.Join(",", requirementLine.S)}]. Treating slot [{dayIndex},{periodIndex}] as unavailable.");
                    return false;
                }

                return entity.jC.Get(dayIndex, periodIndex) == 0;
            });


            if (!allRequiredEntitiesAvailable)
            {
                matrixE.Set(dayIndex, periodIndex, 1);
                continue;
            }


            var constraint = periodIndex >= 0 && periodIndex < requirementLine.Z.Count
                ? requirementLine.Z[periodIndex]
                : 1;

            if (constraint == 0)
                matrixE.Set(dayIndex, periodIndex, 1);
            else
                matrixE.Set(dayIndex, periodIndex, 0);
        }
    }


    private static bool Schedule(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities,
        int numDays, int numPeriods)
    {
        var matrixE = requirementLine.E;


        for (var dayIndex = 0; dayIndex < numDays; dayIndex++)
        for (var periodIndex = 0; periodIndex < numPeriods; periodIndex++)
            if (matrixE.Get(dayIndex, periodIndex) == 0)
                if (ValidatePeriodAvailability(dayIndex, periodIndex, requirementLine, entities))
                {
                    UpdateEAvailability(dayIndex, periodIndex, requirementLine, entities);
                    requirementLine.R.Add(dayIndex, periodIndex);
                    return true;
                }

        return false;
    }


    private static bool ValidatePeriodAvailability(int dayIndex, int startPeriodIndex,
        SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities)
    {
        var numPeriods = requirementLine.E.Columns;


        var actualPeriodsRemaining = numPeriods - startPeriodIndex;
        if (actualPeriodsRemaining < requirementLine.length) return false;


        for (var k = 0; k < requirementLine.length; k++)
        {
            var currentPeriodIndex = startPeriodIndex + k;


            foreach (var entityId in requirementLine.S)
            {
                var entity = entities.FirstOrDefault(e => e.Id == entityId);
                if (entity == null)
                {
                    Console.WriteLine(
                        $"Error: Required entity ID {entityId} not found in the global entities list during validation.");
                    return false;
                }

                if (entity.jC.Get(dayIndex, currentPeriodIndex) == 1) return false;
            }
        }

        return true;
    }


    private static void UpdateEAvailability(int dayIndex, int startPeriodIndex,
        SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities)
    {
        for (var k = 0; k < requirementLine.length; k++)
        {
            var currentPeriodIndex = startPeriodIndex + k;


            foreach (var entityId in requirementLine.S)
            {
                var entity = entities.FirstOrDefault(e => e.Id == entityId);
                if (entity == null)
                {
                    Console.WriteLine(
                        $"Error: Required entity ID {entityId} not found in the global entities list during jC update.");
                    continue;
                }

                entity.jC.Set(dayIndex, currentPeriodIndex, 1);
            }
        }
    }
}
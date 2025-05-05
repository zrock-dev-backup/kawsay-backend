// Scheduling/SchedulingAlgorithm.cs

using KawsayApiMockup.Scheduling;

namespace kawsay.Scheduling
{
    // Contains the core logic of the Yule (1968) scheduling algorithm.
    public static class SchedulingAlgorithm
    {
         // Need access to dayOrder for sorting consistency when mapping indices
         public static readonly List<string> dayOrder = new List<string> {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};

        // The main handler for a single requirement line.
        // Tries to schedule 'q' occurrences for the given requirement line.
        public static bool Handler(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays, int numPeriods)
        {
             // Re-initialize E matrix for this requirement at the start of each Handler call
             // to calculate availability based on the current state of jC matrices.
             requirementLine.E = new SchedulingMatrix(numDays, numPeriods);

             // Try to schedule 'q' occurrences for this requirement
             for (int i = 0; i < requirementLine.q; i++)
             {
                 PopulateEMatrix(requirementLine, entities, numDays, numPeriods); // Update E based on current jC and Z
                 if (!Schedule(requirementLine, entities, numDays, numPeriods)) // Try to schedule ONE occurrence
                 {
                     // If we fail to schedule even one occurrence out of 'q' attempts for this requirement,
                     // then this requirement cannot be fully satisfied in this attempt cycle.
                     System.Console.WriteLine($"Failed to schedule occurrence {i + 1}/{requirementLine.q} for requirement S={string.Join(",", requirementLine.S)}.");
                     return false; // Indicate failure for this requirement
                 }
                 // If Schedule returns true, one occurrence was successfully placed, and jC was updated.
                 // The loop continues to try and schedule the next occurrence for this requirement.
             }

            return true; // All 'q' occurrences for this requirement were successfully scheduled
        }

        // Populates the Availability Matrix (E) for a given requirement line.
        // E[day, period] = 0 if the slot is potentially available for this requirement, 1 otherwise.
        private static void PopulateEMatrix(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays, int numPeriods)
        {
            var matrixE = requirementLine.E;

            for (var dayIndex = 0; dayIndex < numDays; dayIndex++)
            {
                for (var periodIndex = 0; periodIndex < numPeriods; periodIndex++)
                {
                    // Check if all entities required by this requirement (requirementLine.S)
                    // are available (jC.Get == 0) at this specific day/period slot.
                    var allRequiredEntitiesAvailable = requirementLine.S.All(requiredEntityId =>
                    {
                         var entity = entities.FirstOrDefault(e => e.Id == requiredEntityId);
                         if (entity == null)
                         {
                              System.Console.WriteLine($"Error: Required entity ID {requiredEntityId} not found in global entities list during E matrix population.");
                              return false; // Required entity doesn't exist, treat as unavailable
                         }
                         return entity.jC.Get(dayIndex, periodIndex) == 0; // Check if entity's jC is 0 (available)
                    });


                    if (!allRequiredEntitiesAvailable)
                    {
                        matrixE.Set(dayIndex, periodIndex, 1); // Mark slot as unavailable in E if any required entity is busy
                        continue; // Move to next slot
                    }

                    // If entities are available, check against the requirement's Z constraint for this period.
                    // Z constraints define periods that are inherently unavailable for this requirement type.
                    var constraint = (periodIndex >= 0 && periodIndex < requirementLine.Z.Count) ? requirementLine.Z[periodIndex] : 1; // Handle Z out of bounds defensively

                    if (constraint == 0) // If Z[periodIndex] is 0, the slot is constrained by the requirement itself (unavailable)
                    {
                         matrixE.Set(dayIndex, periodIndex, 1); // Mark slot as unavailable in E
                    }
                    else
                    {
                        // If all required entities are available AND no Z constraint applies,
                        // the slot is potentially available for this requirement.
                        matrixE.Set(dayIndex, periodIndex, 0);
                    }
                }
            }
        }


        // Attempts to find and schedule a single occurrence for the requirement line.
        // Returns true if a slot is found and scheduled, false otherwise.
        private static bool Schedule(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays, int numPeriods)
        {
            var matrixE = requirementLine.E; // Availability matrix for this requirement

            // Iterate through the grid to find the first available slot (E[day, period] == 0)
            for (var dayIndex = 0; dayIndex < numDays; dayIndex++)
            {
                for (var periodIndex = 0; periodIndex < numPeriods; periodIndex++)
                {
                    if (matrixE.Get(dayIndex, periodIndex) == 0) // Found a potentially available start slot
                    {
                        // Now, validate if this slot and the 'length' subsequent periods
                        // are actually available *for all involved entities* in their jC matrices.
                        if (ValidatePeriodAvailability(dayIndex, periodIndex, requirementLine, entities))
                        {
                            // If valid, schedule this occurrence:
                            UpdateEAvailability(dayIndex, periodIndex, requirementLine, entities); // Mark periods as busy in jC for involved entities
                            requirementLine.R.Add(dayIndex, periodIndex); // Record the scheduled slot (using 0-based day/period index)
                            return true; // Successfully scheduled one occurrence
                        }
                    }
                }
            }

            return false; // Could not find an available slot for this occurrence after checking all possibilities
        }

        // Validates if a potential slot (startDayIndex, startPeriodIndex) and its 'length' subsequent periods
        // are available for all entities involved in the requirement line, by checking their jC matrices.
        private static bool ValidatePeriodAvailability(int dayIndex, int startPeriodIndex, SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities)
        {
            var numPeriods = requirementLine.E.Columns; // Get periods dimension from matrix E

            // Check if the required length fits within the remaining periods of the day
            var actualPeriodsRemaining = numPeriods - startPeriodIndex;
            if (actualPeriodsRemaining < requirementLine.length)
            {
                return false; // Not enough periods left in the day for the required length
            }

            // Check availability for all involved entities (requirementLine.S)
            // across all periods the occurrence would occupy (from startPeriodIndex to startPeriodIndex + length - 1).
            for (int k = 0; k < requirementLine.length; k++)
            {
                int currentPeriodIndex = startPeriodIndex + k;
                // Matrix.Get handles bounds checking, but this loop structure implies we won't go out of bounds horizontally
                // if the `actualPeriodsRemaining` check passed.

                foreach (var entityId in requirementLine.S) // For each entity required by this line
                {
                    var entity = entities.FirstOrDefault(e => e.Id == entityId);
                    if (entity == null)
                    {
                         System.Console.WriteLine($"Error: Required entity ID {entityId} not found in the global entities list during validation.");
                         return false; // Required entity doesn't exist, cannot validate availability
                    }
                    // Check the entity's jC matrix for unavailability (jC == 1 means busy)
                    if (entity.jC.Get(dayIndex, currentPeriodIndex) == 1)
                    {
                        return false; // Entity is busy during this period, slot is not available
                    }
                }
            }

            return true; // All required entities are available for the entire duration of the potential occurrence
        }


        // Updates the jC matrices of the involved entities to mark the scheduled slot as busy.
        private static void UpdateEAvailability(int dayIndex, int startPeriodIndex, SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities)
        {
            // Mark the scheduled periods as busy (jC = 1) for all involved entities (requirementLine.S)
            for (int k = 0; k < requirementLine.length; k++)
            {
                int currentPeriodIndex = startPeriodIndex + k;
                // Matrix.Set handles bounds checking.

                foreach (var entityId in requirementLine.S) // For each entity required by this line
                {
                    var entity = entities.FirstOrDefault(e => e.Id == entityId);
                     if (entity == null) continue; // Required entity doesn't exist, skip updating its jC

                    entity.jC.Set(dayIndex, currentPeriodIndex, 1); // Mark entity as busy in this slot
                }
            }
        }

        // The original code had a ResetJc function. In this EF Core version,
        // the jC matrices are part of the in-memory SchedulingEntity objects
        // created for a single generation run. Resetting is handled by re-initializing
        // them and potentially repopulating fixed constraints at the start of each
        // full attempt cycle in the SchedulingService.
    }
}

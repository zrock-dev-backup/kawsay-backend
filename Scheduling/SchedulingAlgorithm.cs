// Scheduling/SchedulingAlgorithm.cs

using System.Collections.Generic;
using System.Linq;

namespace KawsayApiMockup.Scheduling // Ensure correct namespace for your project
{
    // Contains the core logic of the Yule (1968) scheduling algorithm.
    // These methods operate on the algorithm's internal data structures.
    public static class SchedulingAlgorithm
    {
         // Need access to dayOrder for sorting consistency when mapping indices back to DB IDs later.
         // This list itself isn't used by the algorithm's logic, but its order is important for the service
         // to correctly interpret the 0-based day indices returned by the algorithm's results (ScheduledTimeslotResult.R).
         public static readonly List<string> dayOrder = new List<string> {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};

        // The main handler for a single requirement line.
        // Tries to schedule 'q' occurrences for the given requirement line in one attempt cycle.
        // Returns true if all 'q' occurrences were successfully scheduled, false otherwise.
        public static bool Handler(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays, int numPeriods)
        {
             // Re-initialize E matrix for this requirement at the start of each Handler call
             // to calculate availability based on the current state of jC matrices.
             requirementLine.E = new SchedulingMatrix(numDays, numPeriods);

             // Try to schedule 'q' occurrences for this requirement
             for (int i = 0; i < requirementLine.q; i++)
             {
                 // 1. Populate the Availability Matrix (E) for this requirement
                 // E[day, period] = 0 if the slot is potentially available for this requirement, 1 otherwise.
                 PopulateEMatrix(requirementLine, entities, numDays, numPeriods);

                 // 2. Attempt to find and schedule ONE occurrence for this requirement
                 // This searches the E matrix for the first available slot and validates it against jC.
                 if (!Schedule(requirementLine, entities, numDays, numPeriods))
                 {
                     // If we fail to schedule even one occurrence out of 'q' attempts for this requirement,
                     // then this requirement cannot be fully satisfied in this attempt cycle.
                     System.Console.WriteLine($"Failed to schedule occurrence {i + 1}/{requirementLine.q} for requirement S=[{string.Join(",", requirementLine.S)}] (q={requirementLine.q}, len={requirementLine.length}).");
                     // The requirement's R list will contain any occurrences successfully scheduled *before* this failure.
                     return false; // Indicate failure for this requirement
                 }
                 // If Schedule returns true, one occurrence was successfully placed, and jC was updated.
                 // The loop continues to try and schedule the next occurrence for this requirement.
             }

            // If the loop completes, all 'q' occurrences for this requirement were successfully scheduled.
            return true; // All 'q' occurrences for this requirement were successfully scheduled
        }

        // Populates the Availability Matrix (E) for a given requirement line.
        // E[day, period] = 0 if the slot is potentially available for this requirement, 1 otherwise.
        // Availability is determined by checking the jC matrix of all involved entities (S)
        // and the requirement's inherent period constraints (Z).
        private static void PopulateEMatrix(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays, int numPeriods)
        {
            var matrixE = requirementLine.E;
            // The E matrix is re-initialized at the start of the Handler call
            // requirementLine.E = new SchedulingMatrix(numDays, numPeriods); // This happens in Handler now

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
                              // This indicates an issue with how S was populated in the Factory.
                              // A required entity doesn't exist in the global entities list.
                              System.Console.WriteLine($"Error: Required entity ID {requiredEntityId} not found in global entities list during E matrix population for requirement S=[{string.Join(",", requirementLine.S)}]. Treating slot [{dayIndex},{periodIndex}] as unavailable.");
                              return false; // Required entity doesn't exist, treat the slot as unavailable for this requirement
                         }
                         // Check if the entity's jC matrix is 0 (available) at this slot
                         return entity.jC.Get(dayIndex, periodIndex) == 0;
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
                        // the slot is potentially available for this requirement (mark with 0 in E).
                        matrixE.Set(dayIndex, periodIndex, 0);
                    }
                }
            }
        }


        // Attempts to find and schedule a single occurrence for the requirement line.
        // It searches the requirement's E matrix for the first available slot (0)
        // and then validates if that slot and the subsequent periods are truly available
        // by checking the jC matrices of all involved entities.
        // Returns true if a valid slot is found and scheduled, false otherwise.
        private static bool Schedule(SchedulingRequirementLine requirementLine, List<SchedulingEntity> entities, int numDays, int numPeriods)
        {
            var matrixE = requirementLine.E; // Availability matrix for this requirement

            // Iterate through the grid (E matrix) row by row, column by column
            // to find the first potentially available start slot (E[day, period] == 0)
            for (var dayIndex = 0; dayIndex < numDays; dayIndex++)
            {
                for (var periodIndex = 0; periodIndex < numPeriods; periodIndex++)
                {
                    if (matrixE.Get(dayIndex, periodIndex) == 0) // Found a potentially available start slot (passed E & Z constraints)
                    {
                        // Now, perform a detailed check: validate if this slot and the 'length' subsequent periods
                        // are actually available *for all involved entities* by checking their jC matrices.
                        if (ValidatePeriodAvailability(dayIndex, periodIndex, requirementLine, entities))
                        {
                            // If the slot is valid for all entities for the required duration:
                            UpdateEAvailability(dayIndex, periodIndex, requirementLine, entities); // Mark periods as busy in jC for involved entities
                            requirementLine.R.Add(dayIndex, periodIndex); // Record the scheduled slot (using 0-based day/period index)
                            return true; // Successfully scheduled ONE occurrence. Exit and let Handler try the next occurrence.
                        }
                         // If ValidatePeriodAvailability returned false, this specific slot starting here is not truly available.
                         // The loop continues to the next period/day in the search for an available slot.
                    }
                }
            }

            // If the loops complete without finding and scheduling a slot:
            return false; // Could not find any available slot for this occurrence in the current state of jC matrices.
        }

        // Validates if a potential slot (startDayIndex, startPeriodIndex) and its 'length' subsequent periods
        // are available for all entities involved in the requirement line, by checking their jC matrices.
        // This checks if any required entity is already busy during the entire proposed occurrence duration.
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
                // The loop structure and the initial `actualPeriodsRemaining` check ensure `currentPeriodIndex` is within bounds horizontally.
                // Vertical bounds check is handled by Matrix.Get.

                foreach (var entityId in requirementLine.S) // For each entity required by this line
                {
                    var entity = entities.FirstOrDefault(e => e.Id == entityId);
                    if (entity == null)
                    {
                         // This indicates an issue with how S was populated in the Factory.
                         // A required entity doesn't exist in the global entities list.
                         System.Console.WriteLine($"Error: Required entity ID {entityId} not found in the global entities list during validation.");
                         return false; // Required entity doesn't exist, cannot validate availability
                    }
                    // Check the entity's jC matrix for unavailability (jC.Get == 1 means busy)
                    if (entity.jC.Get(dayIndex, currentPeriodIndex) == 1)
                    {
                        return false; // Entity is busy during this period, slot is not available for this requirement
                    }
                }
            }

            return true; // All required entities are available for the entire duration of the potential occurrence
        }


        // Updates the jC matrices of the involved entities to mark the scheduled slot as busy.
        // This reflects that these entities are now occupied during this time.
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
                     if (entity == null)
                     {
                          // This indicates an issue with how S was populated in the Factory.
                          System.Console.WriteLine($"Error: Required entity ID {entityId} not found in the global entities list during jC update.");
                          continue; // Required entity doesn't exist, skip updating its jC
                     }

                    entity.jC.Set(dayIndex, currentPeriodIndex, 1); // Mark entity as busy in this slot
                }
            }
        }
    }
}
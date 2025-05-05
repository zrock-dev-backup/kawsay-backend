// Scheduling/ScheduledTimeslotResult.cs

using System.Collections.Generic;

namespace KawsayApiMockup.Scheduling // Ensure correct namespace for your project
{
    // Stores the results of the scheduling algorithm for a specific requirement line.
    // Maps an internal index (0 to q-1 for the requirement's occurrences)
    // to a scheduled day/period index pair.
    public class ScheduledTimeslotResult
    {
        // Dictionary storing the scheduled slots: occurrence_number -> [day_index, period_index] (0-based indices)
        public readonly Dictionary<int, List<int>> R = new();
        private int occurrenceIndex; // Counter for the scheduled occurrences for this requirement (starts at 0)

        // Adds a successfully scheduled slot (day and period indices) to the results.
        public void Add(int dayIndex, int periodIndex)
        {
            R.Add(occurrenceIndex++, new List<int> { dayIndex, periodIndex });
        }

        // Optional: Method to clear results if needed for re-scheduling within an attempt cycle
        public void Clear()
        {
            R.Clear();
            occurrenceIndex = 0; // Reset counter
        }
    }
}

// Scheduling/ScheduledTimeslotResult.cs

using System.Collections.Generic;

namespace KawsayApiMockup.Scheduling
{
    // Stores the results of the scheduling algorithm for a specific requirement line.
    // Maps an internal index to a scheduled day/period pair.
    public class ScheduledTimeslotResult
    {
        // Dictionary storing the scheduled slots: internal_index -> [day_index, period_index]
        public readonly Dictionary<int, List<int>> R = new();
        private int occurrenceIndex; // Counter for the scheduled occurrences for this requirement

        // Adds a scheduled slot (day and period indices) to the results.
        public void Add(int dayIndex, int periodIndex)
        {
            R.Add(occurrenceIndex++, new List<int> { dayIndex, periodIndex });
        }
    }
}

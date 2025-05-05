// Scheduling/SchedulingRequirementLine.cs

using System.Collections.Generic;
using System.Linq; // Needed for Enumerable.Repeat

namespace KawsayApiMockup.Scheduling
{
    // Abstract base class representing a scheduling requirement for the algorithm.
    public abstract class SchedulingRequirementLine
    {
        public List<int> S { get; set; } // List of IDs of the SchedulingEntities involved in this requirement
        public int q { get; set; } // The quantity (number of occurrences) to schedule for this requirement
        public ScheduledTimeslotResult R { get; set; } // The results: list of scheduled slots for this requirement
        public List<int> Z { get; set; } // Period Constraints: 0 means unavailable in that period, 1 means potentially available
        public int length { get; set; } // The length (number of periods) of each occurrence for this requirement
        public SchedulingMatrix E { get; set; } // The Availability Matrix for this requirement (calculated by algorithm)

        // Constructor requires quantity, length, involved entities, and timetable dimensions
        public SchedulingRequirementLine(int q, int length, List<int> s, int numDays, int numPeriods)
        {
            this.q = q;
            this.length = length;
            S = s;
            R = new ScheduledTimeslotResult(); // Initialize results
            E = new SchedulingMatrix(numDays, numPeriods); // Initialize Availability Matrix with correct dimensions
            Z = Enumerable.Repeat(1, numPeriods).ToList(); // Initialize Z constraints (all periods available by default)
        }

        // Helper to set a specific period constraint in the Z list
        protected void SetZ(int periodIndex, int value)
        {
            if (periodIndex >= 0 && periodIndex < Z.Count)
            {
                 Z[periodIndex] = value;
            } else {
                 System.Console.WriteLine($"Warning: Attempted to set Z[{periodIndex}] out of bounds. Z size: {Z.Count}.");
            }
        }

        // Helper to set a range of period constraints in the Z list
        protected void SetZRange(int startPeriodIndex, int endPeriodIndex, int value)
        {
             // Note: Original code had end += 1, assuming end is inclusive. Keeping that convention.
            endPeriodIndex += 1;
            for (int i = startPeriodIndex; i < endPeriodIndex; i++)
            {
                if (i >= 0 && i < Z.Count)
                {
                    Z[i] = value;
                } else {
                     System.Console.WriteLine($"Warning: Attempted to set Z range [{startPeriodIndex}-{endPeriodIndex}] out of bounds. Index {i} out of {Z.Count}.");
                }
            }
        }
    }
}

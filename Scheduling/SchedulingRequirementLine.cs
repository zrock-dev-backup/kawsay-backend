// Scheduling/SchedulingRequirementLine.cs

using System.Collections.Generic;
using System.Linq; // Needed for Enumerable.Repeat

namespace KawsayApiMockup.Scheduling // Ensure correct namespace for your project
{
    // Abstract base class representing a scheduling requirement for the algorithm.
    // Each instance defines a set of entities (S) that need a quantity (q) of occurrences
    // of a specific length (length) to be scheduled, subject to period constraints (Z).
    public abstract class SchedulingRequirementLine
    {
        // --- Properties ---
        public List<int> S { get; set; } // List of IDs of the SchedulingEntities involved in this requirement
        public int q { get; set; } // The quantity (number of occurrences) to schedule for this requirement
        public ScheduledTimeslotResult R { get; set; } // The results: list of scheduled slots for this requirement (populated by the algorithm)
        public List<int> Z { get; set; } // Period Constraints: 0 means period is unavailable for *this requirement*, 1 means potentially available
        public int length { get; set; } // The length (number of consecutive periods) of each occurrence for this requirement
        public SchedulingMatrix E { get; set; } // The Availability Matrix for *this requirement* (calculated by algorithm)


        // --- Constructor ---
        // Requires quantity, length, involved entities, and timetable dimensions (for matrix initialization)
        public SchedulingRequirementLine(int q, int length, List<int> s, int numDays, int numPeriods)
        {
            this.q = q;
            this.length = length;
            S = s ?? new List<int>(); // Initialize S list, handle null input
            R = new ScheduledTimeslotResult(); // Initialize results object
            E = new SchedulingMatrix(numDays, numPeriods); // Initialize Availability Matrix with correct dimensions
            Z = Enumerable.Repeat(1, numPeriods).ToList(); // Initialize Z constraints (all periods available by default)
        }

        // --- Helper Methods for setting Z constraints ---
        // These are protected so concrete subclasses can define their specific constraints.

        // Helper to set a specific period constraint in the Z list (0-based period index)
        protected void SetZ(int periodIndex, int value)
        {
            if (periodIndex >= 0 && periodIndex < Z.Count)
            {
                 Z[periodIndex] = value;
            } else {
                 // Log a warning if attempting to set constraints outside the defined period range
                 System.Console.WriteLine($"Warning: Attempted to set Z[{periodIndex}] out of bounds. Z size: {Z.Count}.");
            }
        }

        // Helper to set a range of period constraints in the Z list (0-based period indices, end inclusive)
        protected void SetZRange(int startPeriodIndex, int endPeriodIndex, int value)
        {
             // Note: Original code had end += 1, assuming end is inclusive. Keeping that convention.
            int inclusiveEndIndex = endPeriodIndex + 1;
            for (int i = startPeriodIndex; i < inclusiveEndIndex; i++)
            {
                if (i >= 0 && i < Z.Count)
                {
                    Z[i] = value;
                } else {
                     // Log a warning if attempting to set constraints outside the defined period range
                     System.Console.WriteLine($"Warning: Attempted to set Z range [{startPeriodIndex}-{endPeriodIndex}] out of bounds. Index {i} out of {Z.Count}.");
                }
            }
        }

        // Abstract method: Concrete requirements *could* override logic if needed,
        // but the core algorithm logic is in the static SchedulingAlgorithm class.
        // This abstract class primarily serves to hold the data (S, q, R, Z, length, E)
        // for a specific requirement instance.
    }
}

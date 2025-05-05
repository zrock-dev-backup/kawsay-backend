// Scheduling/SchedulingEntity.cs

namespace KawsayApiMockup.Scheduling // Ensure correct namespace for your project
{
    // Represents an entity (resource like Teacher, or even the class itself)
    // that needs to be tracked for availability during scheduling.
    public class SchedulingEntity
    {
        public readonly int Id; // Unique ID for this entity within the scheduling context
        public readonly string Name; // Name for debugging/logging
        public SchedulingMatrix jC; // The "Joint Constraints" matrix for this entity (1 = busy, 0 = available)

        // Constructor now takes timetable dimensions to initialize the jC matrix
        public SchedulingEntity(int id, string name, int numDays, int numPeriods)
        {
            Id = id;
            Name = name;
            jC = new SchedulingMatrix(numDays, numPeriods); // Initialize jC matrix based on timetable dimensions
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

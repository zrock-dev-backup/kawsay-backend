using Application.Features.Scheduling.Algorithm;
using Application.Features.Scheduling.Models;

namespace Kawsay.UnitTests.Scheduling;

public class YuleAlgorithmSchedulingTests
{
    
    [Fact]
    public void YuleAlgorithm_ShouldScheduleClassesWithoutConflicts()
    {
        // Arrange
        const int numDays = 5;
        const int numPeriods = 10;
    
        // Create test entities (1 teacher and 2 classes)
        var entities = new List<SchedulingEntity>
        {
            new(1, "Teacher A", numDays, numPeriods),
            new(2, "Class 1", numDays, numPeriods),
            new(3, "Class 2", numDays, numPeriods)
        };

        // Create a requirement line for a 2-period class that should occur twice
        var requirementLine = new SchedulingRequirementLine(2, 2, [1, 2], numDays, numPeriods);
    
        // Act
        var result = YuleAlgorithm.Handler(requirementLine, entities, numDays, numPeriods);
    
        // Assert
        Assert.True(result, "Algorithm should successfully schedule the class");
        Assert.Equal(2, requirementLine.AssignedTimeslotList.Count);
    
        // Verify no time slot conflicts
        var scheduledSlots = new HashSet<(int day, int period)>();
        foreach (var slot in requirementLine.AssignedTimeslotList)
        {
            // Check first period
            Assert.False(scheduledSlots.Contains((slot.Day, slot.Period)), 
                $"Found conflicting schedule at day {slot.Day}, period {slot.Period}");
            scheduledSlots.Add((slot.Day, slot.Period));
        
            // Check second period (class length is 2)
            Assert.False(scheduledSlots.Contains((slot.Day, slot.Period + 1)), 
                $"Found conflicting schedule at day {slot.Day}, period {slot.Period + 1}");
            scheduledSlots.Add((slot.Day, slot.Period + 1));
        
            // Verify teacher availability was marked as occupied
            Assert.Equal(1, entities[0].AvailabilityMatrix.Get(slot.Day, slot.Period));
            Assert.Equal(1, entities[0].AvailabilityMatrix.Get(slot.Day, slot.Period + 1));
        }
    }

    [Fact]
    public void YuleAlgorithm_ShouldRespectPreExistingAvailability()
    {
        // Arrange
        const int numDays = 5;
        const int numPeriods = 10;
    
        var entities = new List<SchedulingEntity>
        {
            new(1, "Teacher A", numDays, numPeriods),
            new(2, "Class 1", numDays, numPeriods)
        };
    
        // Mark teacher as unavailable for first 2 periods of day 0
        entities[0].AvailabilityMatrix.Set(0, 0, 1);
        entities[0].AvailabilityMatrix.Set(0, 1, 1);
    
        var requirementLine = new SchedulingRequirementLine(1, 2, [1, 2], numDays, numPeriods);
    
        // Act
        var result = YuleAlgorithm.Handler(requirementLine, entities, numDays, numPeriods);
    
        // Assert
        Assert.True(result, "Algorithm should successfully schedule the class");
        Assert.Single(requirementLine.AssignedTimeslotList);
    
        var scheduledSlot = requirementLine.AssignedTimeslotList.First();
        Assert.False(scheduledSlot.Day == 0 && (scheduledSlot.Period == 0 || scheduledSlot.Period == 1),
            "Class should not be scheduled in unavailable time slots");
    }
}
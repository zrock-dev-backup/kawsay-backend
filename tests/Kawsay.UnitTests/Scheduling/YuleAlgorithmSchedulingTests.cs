using Application.Features.Scheduling.Algorithm;
using Application.Features.Scheduling.Models;

namespace Kawsay.UnitTests.Scheduling;

public class YuleAlgorithmSchedulingTests
{
    [Fact]
    public void YuleAlgorithm_ShouldScheduleClassesOnPreferredDay()
    {
        // Arrange
        const int numDays = 2;
        const int numPeriods = 5;

        var entities = new List<SchedulingEntity>
        {
            new(1, "Teacher A", numDays, numPeriods),
            new(2, "Class 1", numDays, numPeriods)
        };

        var requirementLine = new SchedulingRequirementLine(1, 2, [1, 2], numDays, numPeriods);
        requirementLine.PeriodPreferenceMatrix.Set(1, 0, 0);
        requirementLine.PeriodPreferenceMatrix.Set(1, 1, 0);

        // Act
        var result = YuleAlgorithm.Handler(requirementLine, entities, numDays, numPeriods);

        // Assert
        Assert.True(result, "Algorithm should successfully schedule the class");
        Assert.Single(requirementLine.AssignedTimeslotList);

        var scheduledSlot = requirementLine.AssignedTimeslotList.First();
        Assert.Equal(1, scheduledSlot.Day); // Should be scheduled on the preferred day
        Assert.Equal(0, scheduledSlot.Period); // Should be scheduled in the preferred period
    }
}
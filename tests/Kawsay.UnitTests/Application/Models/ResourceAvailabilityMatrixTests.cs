using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

namespace Kawsay.UnitTests.Application.Models;

public class ResourceAvailabilityMatrixTests
{
    private readonly TimetableEntity _timetable = new()
    {
        Days =
        [
            new TimetableDayEntity { Id = 1, Name = "Monday" },
            new TimetableDayEntity { Id = 2, Name = "Tuesday" }
        ],
        Periods =
        [
            new TimetablePeriodEntity { Id = 101, Start = "09:00" },
            new TimetablePeriodEntity { Id = 102, Start = "10:00" },
            new TimetablePeriodEntity { Id = 103, Start = "11:00" }
        ]
    };

    [Fact]
    public void HasClash_WhenNewClassHasNoOverlap_ReturnsFalse()
    {
        // Arrange
        var existingClass = new ClassEntity
        {
            Length = 1,
            ClassOccurrences =
            [
                new ClassOccurrenceEntity { Date = new DateOnly(2024, 6, 10), StartPeriodId = 101 } // Monday @ 9am
            ]
        };

        var newClass = new ClassEntity
        {
            Length = 1,
            ClassOccurrences =
            [
                new ClassOccurrenceEntity { Date = new DateOnly(2024, 6, 11), StartPeriodId = 101 } // Tuesday @ 9am
            ]
        };

        var matrix = new ResourceAvailabilityMatrix(_timetable, new List<ISchedulable> { existingClass });

        // Act
        var result = matrix.HasClash(newClass);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasClash_WhenNewClassDirectlyOverlaps_ReturnsTrue()
    {
        // Arrange
        var existingClass = new ClassEntity
        {
            Length = 1,
            ClassOccurrences = [new ClassOccurrenceEntity { Date = new DateOnly(2024, 6, 10), StartPeriodId = 101 }]
        };

        var newClass = new ClassEntity
        {
            Length = 1,
            ClassOccurrences = [new ClassOccurrenceEntity { Date = new DateOnly(2024, 6, 10), StartPeriodId = 101 }]
        };

        var matrix = new ResourceAvailabilityMatrix(_timetable, new List<ISchedulable> { existingClass });

        // Act
        var result = matrix.HasClash(newClass);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasClash_WhenMultiPeriodClassOverlaps_ReturnsTrue()
    {
        // Arrange
        var existingClass = new ClassEntity
        {
            Length = 2, // 9am - 11am on Monday
            ClassOccurrences = [new ClassOccurrenceEntity { Date = new DateOnly(2024, 6, 10), StartPeriodId = 101 }]
        };

        var newClass = new ClassEntity
        {
            Length = 1,
            ClassOccurrences =
            [
                new ClassOccurrenceEntity { Date = new DateOnly(2024, 6, 10), StartPeriodId = 102 }
            ] // Tries to schedule at 10am
        };

        var matrix = new ResourceAvailabilityMatrix(_timetable, new List<ISchedulable> { existingClass });

        // Act
        var result = matrix.HasClash(newClass);

        // Assert
        Assert.True(result);
    }
}
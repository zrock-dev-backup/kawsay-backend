using Application.Models;
using Application.Services;
using Domain.Entities;

namespace Kawsay.UnitTests.Services;

public class CalendarizationServiceTests
{
    private readonly CalendarizationService _calendarizationService = new();

    [Fact]
    public void ProjectSchedule_WithValidInputs_CreatesCorrectNumberOfOccurrences()
    {
        var timetable = new TimetableEntity
        {
            StartDate = new DateOnly(2024, 10, 28), // A Monday
            EndDate = new DateOnly(2024, 11, 8),   // A Friday
            Days = new List<TimetableDayEntity>
            {
                new() { Id = 1, Name = "Monday" },
                new() { Id = 3, Name = "Wednesday" }
            }
        };

        var abstractSchedules = new List<AbstractClassSchedule>
        {
            new(101, 1, 501), // Class 101 on Monday at Period 501
            new(102, 3, 502)  // Class 102 on Wednesday at Period 502
        };

        // Act
        var result = _calendarizationService.ProjectSchedule(timetable, abstractSchedules);

        // Assert
        // Expected: 2 Mondays, 2 Wednesdays in the date range = 4 occurrences total.
        Assert.Equal(4, result.Count);
        Assert.Equal(2, result.Count(o => o is { ClassId: 101, Date.DayOfWeek: DayOfWeek.Monday }));
        Assert.Equal(2, result.Count(o => o is { ClassId: 102, Date.DayOfWeek: DayOfWeek.Wednesday }));
    }

    [Fact]
    public void ProjectSchedule_WithDateRangeNotIncludingScheduledDays_ReturnsEmptyList()
    {
        // Arrange: A timetable for a weekend, but the abstract schedule is for weekdays.
        var timetable = new TimetableEntity
        {
            StartDate = new DateOnly(2024, 11, 2), // A Saturday
            EndDate = new DateOnly(2024, 11, 3),   // A Sunday
            Days = new List<TimetableDayEntity>
            {
                new() { Id = 1, Name = "Monday" }
            }
        };
        var abstractSchedules = new List<AbstractClassSchedule> { new(101, 1, 501) };

        // Act
        var result = _calendarizationService.ProjectSchedule(timetable, abstractSchedules);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ProjectSchedule_WithEmptyAbstractSchedule_ReturnsEmptyList()
    {
        // Arrange
        var timetable = new TimetableEntity
        {
            StartDate = new DateOnly(2024, 10, 28),
            EndDate = new DateOnly(2024, 11, 8),
            Days = new List<TimetableDayEntity> { new() { Id = 1, Name = "Monday" } }
        };
        var abstractSchedules = new List<AbstractClassSchedule>(); // Empty list

        // Act
        var result = _calendarizationService.ProjectSchedule(timetable, abstractSchedules);

        // Assert
        Assert.Empty(result);
    }
}

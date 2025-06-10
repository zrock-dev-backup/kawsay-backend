using Application.DTOs;
using Application.Features.Scheduling;
using Application.Features.Scheduling.Models;
using Application.Models;
using Domain.Entities;

namespace Kawsay.UnitTests.Scheduling;

public class SchedulingDocumentFactoryTest
{
    private static List<TimetableDayEntity> PrepareTimetableDays()
    {
        return
        [
            new TimetableDayEntity { Id = 1, Name = "Monday" },
            new TimetableDayEntity { Id = 2, Name = "Tuesday" }
        ];
    }

    private static List<TimetablePeriodEntity> PrepareTimetablePeriods(int count)
    {
        var periods = new List<TimetablePeriodEntity>();
        for (var i = 0; i < count; i++)
        {
            periods.Add(new TimetablePeriodEntity { Id = 100 + i, Start = $"{8 + i:00}:00", End = $"{9 + i:00}:00" });
        }

        return periods;
    }

    [Fact]
    public void GetDocument_WithValidInput_ReturnsDocumentWithCorrectPreferenceMatrix()
    {
        // Arrange
        const int classId = 1;
        var days = PrepareTimetableDays();
        var periods = PrepareTimetablePeriods(5);

        var periodPreferences = new List<PeriodPreferenceEntity>
        {
            new() { DayId = 1, StartPeriodId = 101 }, // Prefers Monday, 2nd period
            new() { DayId = 2, StartPeriodId = 103 } // Prefers Tuesday, 4th period
        };

        var classesToSchedule = new List<Class>
        {
            new()
            {
                Id = classId,
                TeacherDto = new TeacherDto { Id = 1 },
                CourseDto = new CourseDto { Id = 1 },
                Frequency = 1,
                Length = 2, // Class is 2 periods long
                PeriodPreferences = periodPreferences
            }
        };

        var allSchedulingEntities = new List<SchedulingEntity>()
        {
            new(1, "Teacher", days.Count, periods.Count),
            new(SchedulingService.ClassEntityIdOffset + classId, "class", days.Count, periods.Count),
        };

        var timetable = new TimetableEntity { Days = days, Periods = periods };
        var factory = new SchedulingDocumentFactory(days, periods);

        // Act
        var requirementDocument = factory.GetDocument(classesToSchedule, allSchedulingEntities, timetable);

        // Assert
        Assert.Single(requirementDocument);
        var requirement = requirementDocument.First();
        var preferenceMatrix = requirement.PeriodPreferenceMatrix;

        // Verify matrix dimensions
        Assert.Equal(days.Count, preferenceMatrix.Rows);
        Assert.Equal(periods.Count, preferenceMatrix.Columns);

        // Verify preferred slots are marked as 0
        // Monday (index 0), Period 101 (index 1) and 102 (index 2) because length is 2
        Assert.Equal(0, preferenceMatrix.Get(0, 1));
        Assert.Equal(0, preferenceMatrix.Get(0, 2));

        // Tuesday (index 1), Period 103 (index 3) and 104 (index 4) because length is 2
        Assert.Equal(0, preferenceMatrix.Get(1, 3));
        Assert.Equal(0, preferenceMatrix.Get(1, 4));

        // Verify a non-preferred slot is marked as 1
        Assert.Equal(1, preferenceMatrix.Get(0, 0));
        Assert.Equal(1, preferenceMatrix.Get(1, 0));
    }
}
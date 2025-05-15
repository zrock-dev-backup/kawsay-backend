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
            new TimetableDayEntity(),
            new TimetableDayEntity()
        ];
    }

    private static List<TimetablePeriodEntity> PrepareTimetablePeriods()
    {
        return
        [
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity(),
            new TimetablePeriodEntity()
        ];
    }

    [Fact]
    public void GetDocument_WithValidInput_ReturnsDocument()
    {
        const int classId = 1;
        var periodPreferences = new List<PeriodPreferenceEntity>
        {
            new()
            {
                ClassId = classId,
                StartPeriodId = 1
            },
            new()
            {
                ClassId = classId,
                StartPeriodId = 5
            },
        };
        var classesToSchedule = new List<Class>
        {
            new()
            {
                Id = classId,
                TeacherDto = new TeacherDto
                {
                    Id = 0,
                    Name = "Teacher",
                    Type = "Professor"
                },
                CourseDto = new CourseDto
                {
                    Id = 1,
                    Name = "Course",
                    Code = "123456789"
                },
                Frequency = 3,
                Length = 4,
                PeriodPreferences = periodPreferences
            }
        };
        var allSchedulingEntities = new List<SchedulingEntity>()
        {
            new(0, "Teacher", 1, 1),
            new(SchedulingService.ClassEntityIdOffset + classId, "class", 1, 1),
        };
        var timetable = new TimetableEntity
        {
            Days = PrepareTimetableDays(),
            Periods = PrepareTimetablePeriods()
        };
        var aux = 0;
        foreach (var period in timetable.Periods)
        {
            period.Id = aux++;
        }

        var schedulingDocumentFactory =
            new SchedulingDocumentFactory(timetable.Periods.ToList(), timetable.Periods.Count);
        var requirementDocument = schedulingDocumentFactory.GetDocument(
            classesToSchedule,
            allSchedulingEntities,
            timetable
        );

        Assert.NotEmpty(requirementDocument);
        Assert.Single(requirementDocument);
        var periodPreference = requirementDocument.First().PeriodPreferenceList;

        Assert.Equal(11, periodPreference.Count);
        for (var i = 1; i < 9; i++)
        {
            Assert.Equal(0, periodPreference[i]);
        }

        Assert.Equal(1, periodPreference[0]);
        Assert.Equal(1, periodPreference[9]);
        Assert.Equal(1, periodPreference[10]);
    }

    [Fact]
    public void MapIdToPeriodIndex_WithValidInput_PopulatesMap()
    {
        var timetable = new TimetableEntity
        {
            Days = PrepareTimetableDays(),
            Periods = PrepareTimetablePeriods()
        };

        var aux = 200;
        foreach (var period in timetable.Periods)
        {
            period.Id = aux++;
        }

        var map = SchedulingDocumentFactory.MapIdToPeriodIndex(timetable.Periods.ToList(),
            timetable.Periods.Count);

        Assert.NotNull(map);

        var keyCollection = map.Keys;
        var valueCollection = map.Values;
        for (var i = 0; i < keyCollection.Count; i++)
        {
            var offset = i + 200;
            Assert.Equal(offset, keyCollection.ElementAt(i));
            Assert.Equal(i, valueCollection.ElementAt(i));
        }
    }
    
    [Fact]
    public void MapIdToPeriodIndex_WithUnSortedInput_PopulatesMap()
    {
        var timetable = new TimetableEntity
        {
            Days = PrepareTimetableDays(),
            Periods = PrepareTimetablePeriods()
        };
        var aux = 210;
        foreach (var period in timetable.Periods)
        {
            period.Id = aux--;
        }

        var map = SchedulingDocumentFactory.MapIdToPeriodIndex(timetable.Periods.ToList(),
            timetable.Periods.Count);
        Assert.NotNull(map);

        var keyCollection = map.Keys;
        var valueCollection = map.Values;
        for (var i = 0; i < keyCollection.Count; i++)
        {
            var offset = i + 200;
            Assert.Equal(offset, keyCollection.ElementAt(i));
            Assert.Equal(i, valueCollection.ElementAt(i));
        }
    }
}
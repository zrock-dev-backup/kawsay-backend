using kawsay.Entities;
using kawsay.Scheduling;
using kawsay.Services;

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
        var periodPreferences = new List<PeriodPreference>
        {
            new(){
                ClassId = classId,
                DayId = 0,
                StartPeriodId = 1
            },
            new(){
                ClassId = classId,
                DayId = 0,
                StartPeriodId = 5
            },
        };
        var classesToSchedule = new List<ClassEntity>
        {
            new()
            {
                Id = classId,
                TeacherId = 0,
                Frequency = 3,
                Length = 4,
                PeriodPreferences = periodPreferences,
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
        

        var requirementDocument = SchedulingDocumentFactory.GetDocument(
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
}
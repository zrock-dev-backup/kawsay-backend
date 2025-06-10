using Application.Features.Scheduling.Models;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Scheduling;

public class SchedulingDocumentFactory
{
    private readonly Dictionary<int, int> _periodIdToIndexMap;
    private readonly Dictionary<int, int> _dayIdToIndexMap;

    public SchedulingDocumentFactory(List<TimetableDayEntity> days, List<TimetablePeriodEntity> periods)
    {
        var sortedDays = days.OrderBy(d => (int)Enum.Parse<DayOfWeek>(d.Name, true)).ToList();
        _dayIdToIndexMap = sortedDays
            .Select((day, index) => new { day.Id, Index = index })
            .ToDictionary(x => x.Id, x => x.Index);

        var sortedPeriods = periods.OrderBy(p => p.Start).ToList();
        _periodIdToIndexMap = sortedPeriods
            .Select((period, index) => new { period.Id, Index = index })
            .ToDictionary(x => x.Id, x => x.Index);
    }

    private void PopulatePeriodPreferences(Class classEntity, SchedulingRequirementLine requirementLine)
    {
        foreach (var preference in classEntity.PeriodPreferences)
        {
            if (!_dayIdToIndexMap.TryGetValue(preference.DayId, out var dayIndex))
            {
                Console.WriteLine(
                    $"Warning: Day ID {preference.DayId} not found in day index map. Skipping preference.");
                continue;
            }

            if (!_periodIdToIndexMap.TryGetValue(preference.StartPeriodId, out var startPeriodIndex))
            {
                Console.WriteLine(
                    $"Warning: Period ID {preference.StartPeriodId} not found in period index map. Skipping preference.");
                continue;
            }

            for (var i = 0; i < classEntity.Length; i++)
            {
                var currentPeriodIndex = startPeriodIndex + i;
                if (currentPeriodIndex < requirementLine.PeriodPreferenceMatrix.Columns)
                {
                    requirementLine.PeriodPreferenceMatrix.Set(dayIndex, currentPeriodIndex, 0); // 0 = Preferred
                }
            }
        }
    }

    public LinkedList<SchedulingRequirementLine> GetDocument(
        List<Class> classesToSchedule,
        List<SchedulingEntity> allSchedulingEntities,
        TimetableEntity timetable
    )
    {
        var document = new LinkedList<SchedulingRequirementLine>();
        foreach (var classEntity in classesToSchedule)
        {
            if (classEntity.PeriodPreferences.Count == 0)
            {
                Console.WriteLine(
                    $"Warning: Class {classEntity.Id} ({classEntity.CourseDto.Name}) has no period preferences. Skipping requirement creation for this class.");
                continue;
            }

            var frequency = classEntity.Frequency;
            var length = classEntity.Length;
            var entityIdsList = new List<int>();

            if (allSchedulingEntities.Any(entity => entity.Id == classEntity.TeacherDto.Id))
                entityIdsList.Add(classEntity.TeacherDto.Id);
            else
                Console.WriteLine(
                    $"Warning: Teacher ID {classEntity.TeacherDto.Id} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping teacher for S list for this requirement.");

            var classSchedulingEntityId = classEntity.Id + SchedulingService.ClassEntityIdOffset;
            if (allSchedulingEntities.Any(entity => entity.Id == classSchedulingEntityId))
            {
                entityIdsList.Add(classSchedulingEntityId);
            }
            else
            {
                Console.WriteLine(
                    $"Error: Class Scheduling Entity ID {classSchedulingEntityId} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping requirement creation for this class.");
                continue;
            }


            var sList = entityIdsList.ToList();
            if (frequency > 0 && length > 0 && sList.Count > 0)
            {
                var requirement = new SchedulingRequirementLine(
                    frequency,
                    length,
                    sList,
                    timetable.Days.Count,
                    timetable.Periods.Count
                );
                PopulatePeriodPreferences(classEntity, requirement);
                document.AddLast(requirement);
            }
            else
            {
                Console.WriteLine(
                    $"Warning: Skipping requirement creation for Class {classEntity.Id} ({classEntity.CourseDto.Name})" +
                    $" due to zero required occurrences ({frequency})/length ({length}) or no involved entities (S list count: {sList.Count}).");
            }
        }

        return document;
    }
}
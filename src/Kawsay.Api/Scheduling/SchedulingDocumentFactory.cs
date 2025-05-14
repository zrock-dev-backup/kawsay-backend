using Api.Services;
using Domain.Entities;

namespace Api.Scheduling;

public class SchedulingDocumentFactory(List<TimetablePeriodEntity> periods, int amntPeriods)
{
    private readonly Dictionary<int, int> _periodIndexMap = MapIdToPeriodIndex(periods, amntPeriods) ?? new Dictionary<int, int>();

    public static Dictionary<int, int>? MapIdToPeriodIndex(List<TimetablePeriodEntity> periods, int amntPeriods)
    {
        if (periods.Count != amntPeriods) {
            Console.WriteLine($"Error: Periods list count ({periods.Count}) does not match period amount ({amntPeriods}).");
            return null;
        }
        periods.Sort();
        var periodIndexMap = new Dictionary<int, int>();
        for (var i = 0; i < periods.Count; i++)
            periodIndexMap.Add(periods[i].Id, i);
        return periodIndexMap;
    }

    private void PopulatePeriodPreferences(ClassEntity classEntity, SchedulingRequirementLine requirementLine)
    {
        foreach (var occurrence in classEntity.ClassOccurrences)
        {
            var start = occurrence.StartPeriodId;
            SetRange(start, start + classEntity.Length);
        }

        return;

        void SetRange(int start, int length)
        {
            for (var i = start; i < length; i++)
            {
                var periodIndex = _periodIndexMap.GetValueOrDefault(i, -1);
                if (periodIndex < 0)
                {
                    Console.WriteLine(
                        $"Warning: Period ID {i} not found in period index map. Skipping occurrence creation for this occurrence.");
                    continue;
                }
                requirementLine.PeriodPreferenceList[periodIndex] = 0;
            }
        }
    }

    public LinkedList<SchedulingRequirementLine> GetDocument(
        List<ClassEntity> classesToSchedule,
        List<SchedulingEntity> allSchedulingEntities,
        TimetableEntity timetable
    )
    {
        var document = new LinkedList<SchedulingRequirementLine>();
        foreach (var classEntity in classesToSchedule)
        {
            if (classEntity.ClassOccurrences.Count == 0)
            {
                Console.WriteLine(
                    $"Warning: Class {classEntity.Id} ({classEntity.Course.Name}) has no period preferences. Skipping requirement creation for this class.");
                continue;
            }

            var frequency = classEntity.Frequency;
            var length = classEntity.Length;
            var entityIdsList = new List<int>();

            if (allSchedulingEntities.Any(entity => entity.Id == classEntity.TeacherId))
                entityIdsList.Add(classEntity.TeacherId);
            else
                Console.WriteLine(
                    $"Warning: Teacher ID {classEntity.TeacherId} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping teacher for S list for this requirement.");

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
                    $"Warning: Skipping requirement creation for Class {classEntity.Id} ({classEntity.Course.Name})" +
                    $" due to zero required occurrences ({frequency})/length ({length}) or no involved entities (S list count: {sList.Count}).");
            }
        }

        return document;
    }
}
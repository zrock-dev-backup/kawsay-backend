using kawsay.Entities;
using kawsay.Services;

namespace kawsay.Scheduling;

public static class SchedulingDocumentFactory
{
    private static void PopulatePeriodPreferences(ClassEntity classEntity, SchedulingRequirementLine requirementLine)
    {
        foreach (var occurrence in classEntity.PeriodPreferences)
        {
            var start = occurrence.StartPeriodId;
            SetRange(start, start + classEntity.Length);
        }

        return;

        void SetRange(int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                if (i >= 0 && i < requirementLine.PeriodPreferenceList.Count)
                    requirementLine.PeriodPreferenceList[i] = 0;
                else
                    Console.WriteLine(
                        $"Warning: Attempted to set Z range [{start}-{end}] out of bounds. Index {i} out of {requirementLine.PeriodPreferenceList.Count}."
                        );
            }
        }
    }

    public static LinkedList<SchedulingRequirementLine> GetDocument(
        List<ClassEntity> classesToSchedule,
        List<SchedulingEntity> allSchedulingEntities,
        TimetableEntity timetable
    )
    {
        var document = new LinkedList<SchedulingRequirementLine>();
        foreach (var classEntity in classesToSchedule)
        {
            var frequency = classEntity.Frequency;
            var length = classEntity.Length;
            var entityIdsList = new List<int>();

            if (classEntity.TeacherId.HasValue)
            {
                if (allSchedulingEntities.Any(entity => entity.Id == classEntity.TeacherId.Value))
                    entityIdsList.Add(classEntity.TeacherId.Value);
                else
                    Console.WriteLine(
                        $"Warning: Teacher ID {classEntity.TeacherId.Value} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping teacher for S list for this requirement.");
            }

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
using kawsay.Entities;
using kawsay.Services;

namespace kawsay.Scheduling;

public static class SchedulingDocumentFactory
{
    public static LinkedList<SchedulingRequirementLine> GetDocument(
        List<ClassEntity> classesToSchedule,
        List<SchedulingEntity> allSchedulingEntities,
        TimetableEntity timetable
    )
    {
        var document = new LinkedList<SchedulingRequirementLine>();


        foreach (var classEntity in classesToSchedule)
        {
            var requiredOccurrenceCount = classEntity.RequiredOccurrenceCount;
            var occurrenceLength = classEntity.OccurrenceLength;


            var sEntityIdsForClass = new List<int>();

            if (classEntity.TeacherId.HasValue)
            {
                if (allSchedulingEntities.Any(se => se.Id == classEntity.TeacherId.Value))
                    sEntityIdsForClass.Add(classEntity.TeacherId.Value);
                else
                    Console.WriteLine(
                        $"Warning: Teacher ID {classEntity.TeacherId.Value} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping teacher for S list for this requirement.");
            }


            var classSchedulingEntityId = classEntity.Id + SchedulingService.ClassEntityIdOffset;

            if (allSchedulingEntities.Any(se => se.Id == classSchedulingEntityId))
            {
                sEntityIdsForClass.Add(classSchedulingEntityId);
            }
            else
            {
                Console.WriteLine(
                    $"Error: Class Scheduling Entity ID {classSchedulingEntityId} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping requirement creation for this class.");
                continue;
            }


            var sList = sEntityIdsForClass.ToList();


            if (requiredOccurrenceCount > 0 && occurrenceLength > 0 && sList.Count > 0)
            {
                var requirement = new GenericSchedulingRequirementLine(
                    requiredOccurrenceCount,
                    occurrenceLength,
                    sList,
                    timetable.Days.Count,
                    timetable.Periods.Count
                );
                document.AddLast(requirement);
            }
            else
            {
                Console.WriteLine(
                    $"Warning: Skipping requirement creation for Class {classEntity.Id} ({classEntity.Course?.Name ?? "Unknown Course"})" +
                    $" due to zero required occurrences ({requiredOccurrenceCount})/length ({occurrenceLength}) or no involved entities (S list count: {sList.Count}).");
            }
        }


        return document;
    }
}
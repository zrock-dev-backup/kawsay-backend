using System.Globalization;
using kawsay.Data;
using kawsay.Entities;
using kawsay.Scheduling;
using Microsoft.EntityFrameworkCore;
using static System.TimeSpan;

namespace kawsay.Services;

public class SchedulingService(KawsayDbContext context)
{
    public const int ClassEntityIdOffset = 10000;

    public async Task<bool> GenerateScheduleAsync(int timetableId)
    {
        Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");
        var timetable = await context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .FirstOrDefaultAsync(t => t.Id == timetableId);
        if (timetable == null) throw new ArgumentException($"Timetable with ID {timetableId} not found.");
        
        var classesToSchedule = await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Where(c => c.TimetableId == timetableId)
            .Where(c => c.Frequency > 0 && c.Length > 0)
            .ToListAsync();

        var allTeachers = await context.Teachers.ToListAsync();
        var allSchedulingEntities = new List<SchedulingEntity>();
        allSchedulingEntities.AddRange(allTeachers.Select(t =>
            new SchedulingEntity(t.Id, t.Name, timetable.Days.Count, timetable.Periods.Count)));
        allSchedulingEntities.AddRange(classesToSchedule.Select(c => new SchedulingEntity(c.Id + ClassEntityIdOffset,
            $"Class {c.Id} ({c.Course.Code})", timetable.Days.Count, timetable.Periods.Count)));

        var requirementDocument = SchedulingDocumentFactory.GetDocument(
            classesToSchedule,
            allSchedulingEntities,
            timetable
        );

        if (requirementDocument.Count == 0)
        {
            Console.WriteLine(
                "No requirements generated for scheduling. Clearing existing schedule for this timetable.");
            // var existingOccurrencesA = await context.ClassOccurrences
            //     .Where(o => context.Classes.Any(c => c.Id == o.ClassId && c.TimetableId == timetableId))
            //     .ToListAsync();
            // context.ClassOccurrences.RemoveRange(existingOccurrencesA);
            // await context.SaveChangesAsync();
            return true;
        }

        var attempts = 0;
        var maxAttempts = 100;
        var currentDocument = new LinkedList<SchedulingRequirementLine>(requirementDocument);
        var enumerator = currentDocument.GetEnumerator();
        InitializeJcMatrices();

        while (enumerator.MoveNext() && attempts < maxAttempts)
        {
            var currentReq = enumerator.Current;
            if (!SchedulingAlgorithm.Handler(currentReq, allSchedulingEntities, timetable.Days.Count,
                    timetable.Periods.Count))
            {
                Console.WriteLine(
                    $"Scheduling failed for requirement S=[{string.Join(",", currentReq.EntitiesList)}] (q={currentReq.Frequency}, len={currentReq.Length}) after {attempts + 1} attempts. Moving to front and backtracking.");
                currentDocument.Remove(currentReq);
                currentDocument.AddFirst(currentReq);
                enumerator = currentDocument.GetEnumerator();
                InitializeJcMatrices();
                attempts++;
            }
            else
            {
                Console.WriteLine(
                    $"Successfully scheduled requirement for S=[{string.Join(",", currentReq.EntitiesList)}]. Results: {currentReq.AssignedTimeslotList.TimeslotDict.Count} occurrences scheduled.");
            }
        }

        if (attempts >= maxAttempts)
        {
            Console.WriteLine(
                $"Scheduling failed after {maxAttempts} attempts. Could not schedule all requirements.");
            return false;
        }

        var classIdsToSchedule = classesToSchedule.Select(c => c.Id).ToList();
        var existingOccurrences = await context.ClassOccurrences
            .Where(o => classIdsToSchedule.Contains(o.ClassId))
            .ToListAsync();
        context.ClassOccurrences.RemoveRange(existingOccurrences);

        var newOccurrences = new List<PeriodPreference>();
        var sortedDays = timetable.Days.OrderBy(d => SchedulingAlgorithm.DayOrder.IndexOf(d.Name)).ToList();
        var sortedPeriods = timetable.Periods.OrderBy(p => ParseExact(p.Start, "HH\\:mm", CultureInfo.InvariantCulture))
            .ToList();
        foreach (var requirement in currentDocument)
        {
            var classEntitySchedulingId = requirement.EntitiesList.FirstOrDefault(id => id >= ClassEntityIdOffset);
            if (classEntitySchedulingId == 0)
            {
                Console.WriteLine(
                    $"Warning: Could not find Class Entity ID in S list (using offset {ClassEntityIdOffset}) for requirement S=[{string.Join(",", requirement.EntitiesList)}]. Skipping occurrence creation for this requirement.");
                continue;
            }

            var classEntityId = classEntitySchedulingId - ClassEntityIdOffset;
            foreach (var resultPair in requirement.AssignedTimeslotList.TimeslotDict.Values)
            {
                var dayIndex = resultPair[0];
                var periodIndex = resultPair[1];
                if (dayIndex < 0 || dayIndex >= sortedDays.Count || periodIndex < 0 ||
                    periodIndex >= sortedPeriods.Count)
                {
                    Console.WriteLine(
                        $"Warning: Algorithm result indices out of bounds for Class Entity ID {classEntityId}: Day index {dayIndex} (max {sortedDays.Count - 1}), Period index {periodIndex} (max {sortedPeriods.Count - 1}). Skipping occurrence creation for this result.");
                    continue;
                }

                var dayEntity = sortedDays[dayIndex];
                var startPeriodEntity = sortedPeriods[periodIndex];
                newOccurrences.Add(new PeriodPreference
                {
                    ClassId = classEntityId,
                    DayId = dayEntity.Id,
                    StartPeriodId = startPeriodEntity.Id
                });
            }

            Console.WriteLine(
                $"Created {requirement.AssignedTimeslotList.TimeslotDict.Count} new occurrences for Class Entity ID {classEntityId}.");
        }
        context.ClassOccurrences.AddRange(newOccurrences);
        await context.SaveChangesAsync();
        Console.WriteLine(
            $"Finished schedule generation for timetable ID: {timetableId}. Overall success: {attempts < maxAttempts}");
        return attempts < maxAttempts;

        void InitializeJcMatrices()
        {
            foreach (var entity in allSchedulingEntities) entity.AvailabilityMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
        }
    }
}
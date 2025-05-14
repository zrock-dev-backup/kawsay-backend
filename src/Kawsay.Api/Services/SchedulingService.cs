using System.Globalization;
using Api.Data;
using Api.Entities;
using Api.Scheduling;
using Api.Scheduling.Utils;
using Microsoft.EntityFrameworkCore;
using static System.TimeSpan;

namespace Api.Services;

public class SchedulingService(KawsayDbContext context)
{
    public const int ClassEntityIdOffset = 10000;

    public async Task<bool> GenerateScheduleAsync(int timetableId)
    {
        Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");

        // Gather data from database
        var timetable = await context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .FirstOrDefaultAsync(t => t.Id == timetableId);
        if (timetable == null) throw new ArgumentException($"Timetable with ID {timetableId} not found.");
        var classesToSchedule = await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.ClassOccurrences)
            .Where(c => c.TimetableId == timetableId)
            .Where(c => c.Frequency > 0 && c.Length > 0)
            .ToListAsync();
        var allTeachers = await context.Teachers.ToListAsync();

        // Populate list of scheduling entities
        var allSchedulingEntities = new List<SchedulingEntity>();
        allSchedulingEntities.AddRange(allTeachers.Select(teacher =>
            new SchedulingEntity(
                teacher.Id,
                teacher.Name,
                timetable.Days.Count,
                timetable.Periods.Count
            )));
        allSchedulingEntities.AddRange(classesToSchedule.Select(lecture =>
            new SchedulingEntity(
                lecture.Id + ClassEntityIdOffset,
                $"Class {lecture.Id} ({lecture.Course.Code})",
                timetable.Days.Count,
                timetable.Periods.Count
            )));

        // Generate requirements document
        var schedulingDocumentFactory =
            new SchedulingDocumentFactory(timetable.Periods.ToList(), timetable.Periods.Count);
        var requirementDocument = schedulingDocumentFactory.GetDocument(
            classesToSchedule,
            allSchedulingEntities,
            timetable
        );
        if (requirementDocument.Count == 0)
        {
            Console.WriteLine(
                "Generated list of requirement documents is empty. No classes to schedule. Skipping schedule generation.");
            return false;
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

        //
        // Store generated schedule
        //

        // Cleanup class old period preferences
        var classIdsToSchedule = classesToSchedule.Select(c => c.Id).ToList();
        var existingOccurrences = await context.PeriodPreferences
            .Where(o => classIdsToSchedule.Contains(o.ClassId))
            .ToListAsync();
        context.PeriodPreferences.RemoveRange(existingOccurrences);

        var newOccurrences = new List<ClassOccurrence>();
        var map = new Dictionary<int, List<int>>();
        foreach (var day in timetable.Days)
        {
            map.Add(day.Id, timetable.Periods.Select(o => o.Id).ToList());
        }

        var mapHelper = new IndexIdMapHelper(timetable.Days.Count, timetable.Periods.Count, map);

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
                var pair = mapHelper.GetId(new Pair(resultPair[1], resultPair[1]));
                var dayIndex = pair.A;
                var periodIndex = pair.B;
                newOccurrences.Add(new ClassOccurrence 
                {
                    
                    ClassId = classEntityId,
                    DayId = dayIndex,
                    StartPeriodId = periodIndex
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
            foreach (var entity in allSchedulingEntities)
                entity.AvailabilityMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
        }
    }
}
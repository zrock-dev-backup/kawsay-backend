using System.Globalization;
using KawsayApiMockup.Data;
using KawsayApiMockup.Entities;
using KawsayApiMockup.Scheduling;
using Microsoft.EntityFrameworkCore;
using static System.TimeSpan;

namespace KawsayApiMockup.Services;

public class SchedulingService
{
    public const int ClassEntityIdOffset = 10000;
    private readonly KawsayDbContext _context;

    public SchedulingService(KawsayDbContext context)
    {
        _context = context;
    }


    public async Task<bool> GenerateScheduleAsync(int timetableId)
    {
        Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");


        var timetable = await _context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .FirstOrDefaultAsync(t => t.Id == timetableId);

        if (timetable == null) throw new ArgumentException($"Timetable with ID {timetableId} not found.");


        var classesToSchedule = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Where(c => c.TimetableId == timetableId)
            .Where(c => c.RequiredOccurrenceCount > 0 && c.OccurrenceLength > 0)
            .ToListAsync();


        var allTeachers = await _context.Teachers.ToListAsync();


        var allSchedulingEntities = new List<SchedulingEntity>();

        allSchedulingEntities.AddRange(allTeachers.Select(t =>
            new SchedulingEntity(t.Id, t.Name, timetable.Days.Count, timetable.Periods.Count)));

        allSchedulingEntities.AddRange(classesToSchedule.Select(c => new SchedulingEntity(c.Id + ClassEntityIdOffset,
            $"Class {c.Id} ({c.Course?.Code ?? "N/A"})", timetable.Days.Count, timetable.Periods.Count)));


        var initializeJcMatrices = () =>
        {
            foreach (var entity in allSchedulingEntities)
                entity.jC = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
        };


        var requirementDocument = SchedulingDocumentFactory.GetDocument(
            classesToSchedule,
            allSchedulingEntities,
            timetable
        );


        if (!requirementDocument.Any())
        {
            Console.WriteLine(
                "No requirements generated for scheduling. Clearing existing schedule for this timetable.");

            var existingOccurrencesA = await _context.ClassOccurrences
                .Where(o => _context.Classes.Any(c => c.Id == o.ClassId && c.TimetableId == timetableId))
                .ToListAsync();
            _context.ClassOccurrences.RemoveRange(existingOccurrencesA);
            await _context.SaveChangesAsync();
            return true;
        }


        var attempts = 0;
        var maxAttempts =
            100;


        var currentDocument = new LinkedList<SchedulingRequirementLine>(requirementDocument);
        var enumerator = currentDocument.GetEnumerator();


        initializeJcMatrices();

        var allScheduledSuccessfully = true;


        while (enumerator.MoveNext() && attempts < maxAttempts)
        {
            var currentReq = enumerator.Current;


            if (!SchedulingAlgorithm.Handler(currentReq, allSchedulingEntities, timetable.Days.Count,
                    timetable.Periods.Count))
            {
                Console.WriteLine(
                    $"Scheduling failed for requirement S=[{string.Join(",", currentReq.S)}] (q={currentReq.q}, len={currentReq.length}) after {attempts + 1} attempts. Moving to front and backtracking.");


                currentDocument.Remove(currentReq);
                currentDocument.AddFirst(currentReq);

                enumerator = currentDocument.GetEnumerator();


                initializeJcMatrices();

                attempts++;

                allScheduledSuccessfully = false;
            }
            else
            {
                Console.WriteLine(
                    $"Successfully scheduled requirement for S=[{string.Join(",", currentReq.S)}]. Results: {currentReq.R.R.Count} occurrences scheduled.");
            }
        }


        if (attempts >= maxAttempts)
        {
            Console.WriteLine(
                $"Scheduling failed after {maxAttempts} attempts. Could not schedule all requirements.");


            return false;
        }


        var classIdsToSchedule = classesToSchedule.Select(c => c.Id).ToList();
        var existingOccurrences = await _context.ClassOccurrences
            .Where(o => classIdsToSchedule.Contains(o.ClassId))
            .ToListAsync();
        _context.ClassOccurrences.RemoveRange(existingOccurrences);


        var newOccurrences = new List<ClassOccurrenceEntity>();


        var sortedDays = timetable.Days.OrderBy(d => SchedulingAlgorithm.dayOrder.IndexOf(d.Name)).ToList();

        var sortedPeriods = timetable.Periods.OrderBy(p => ParseExact(p.Start, "HH\\:mm", CultureInfo.InvariantCulture))
            .ToList();


        foreach (var requirement in currentDocument)
        {
            var classEntitySchedulingId = requirement.S.FirstOrDefault(id => id >= ClassEntityIdOffset);
            if (classEntitySchedulingId == 0)
            {
                Console.WriteLine(
                    $"Warning: Could not find Class Entity ID in S list (using offset {ClassEntityIdOffset}) for requirement S=[{string.Join(",", requirement.S)}]. Skipping occurrence creation for this requirement.");
                continue;
            }

            var classEntityId = classEntitySchedulingId - ClassEntityIdOffset;


            foreach (var resultPair in requirement.R.R.Values)
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


                newOccurrences.Add(new ClassOccurrenceEntity
                {
                    ClassId = classEntityId,
                    DayId = dayEntity.Id,
                    StartPeriodId = startPeriodEntity.Id,
                    Length = requirement.length
                });
            }

            Console.WriteLine($"Created {requirement.R.R.Count} new occurrences for Class Entity ID {classEntityId}.");
        }


        _context.ClassOccurrences.AddRange(newOccurrences);
        await _context.SaveChangesAsync();

        Console.WriteLine(
            $"Finished schedule generation for timetable ID: {timetableId}. Overall success: {attempts < maxAttempts}");


        return attempts < maxAttempts;
    }
}
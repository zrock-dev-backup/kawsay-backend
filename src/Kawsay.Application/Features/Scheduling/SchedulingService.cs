using Application.DTOs;
using Application.Features.Scheduling.Algorithm;
using Application.Features.Scheduling.Models;
using Application.Interfaces.Persistence;
using Application.Models;
using Application.Services;

namespace Application.Features.Scheduling;

public class SchedulingService(
    ITimetableRepository timetableRepository,
    IClassRepository classRepository,
    ITeacherRepository teacherRepository,
    IClassOccurrenceRepository classOccurrenceRepository,
    CalendarizationService calendarizationService
)
{
    public const int ClassEntityIdOffset = 10000;

    public async Task<bool> GenerateScheduleAsync(int timetableId)
    {
        Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");

        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null) throw new ArgumentException($"Timetable with ID {timetableId} not found.");

        var classEntities = await classRepository.GetAllAsync(timetableId);
        if (!classEntities.Any()) throw new ArgumentException($"No classes found for timetable ID {timetableId}.");

        var classIds = classEntities.Select(x => x.Id).ToList();
        await classOccurrenceRepository.DeleteByClassIdAsync(classIds);

        var classesToSchedule = classEntities.Select(entity => new Class
        {
            Id = entity.Id,
            TimetableId = entity.TimetableId,
            CourseDto = new CourseDto
            {
                Id = entity.Course.Id,
                Name = entity.Course.Name,
                Code = entity.Course.Code,
            },
            TeacherDto = new TeacherDto
            {
                Id = entity.Teacher.Id,
                Name = entity.Teacher.Name,
                Type = entity.Teacher.Type,
            },
            Length = entity.Length,
            Frequency = entity.Frequency,
            PeriodPreferences = entity.PeriodPreferences,
        }).ToList();

        var allTeachers = await teacherRepository.GetAllAsync();
        if (!allTeachers.Any()) throw new ArgumentException("No teachers found in database.");

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
                $"Class {lecture.Id} ({lecture.CourseDto.Code})",
                timetable.Days.Count,
                timetable.Periods.Count
            )));
        var entitiesById = allSchedulingEntities.ToDictionary(e => e.Id);
        var sortedPeriods = timetable.Periods.OrderBy(p => p.Start).ToList();
        var schedulingDocumentFactory = new SchedulingDocumentFactory(timetable.Days.ToList(), sortedPeriods);
        var requirementDocument =
            schedulingDocumentFactory.GetDocument(classesToSchedule, allSchedulingEntities, timetable);

        if (requirementDocument.Count == 0)
        {
            Console.WriteLine("Generated list of requirement documents is empty. No classes to schedule.");
            return true;
        }

        var attempts = 0;
        const int maxAttempts = 100;
        var currentDocument = new LinkedList<SchedulingRequirementLine>(requirementDocument);
        var enumerator = currentDocument.GetEnumerator();
        InitializeJcMatrices(entitiesById.Values);

        while (enumerator.MoveNext() && attempts < maxAttempts)
        {
            var currentReq = enumerator.Current;
            if (!YuleAlgorithm.Handler(currentReq, entitiesById, timetable.Days.Count,
                    timetable.Periods.Count))
            {
                Console.WriteLine(
                    $"Scheduling failed for requirement S=[{string.Join(",", currentReq.EntitiesList)}] (q={currentReq.Frequency}, len={currentReq.Length}) after {attempts + 1} attempts. Moving to front and backtracking.");
                currentDocument.Remove(currentReq);
                currentDocument.AddFirst(currentReq);
                enumerator = currentDocument.GetEnumerator();
                InitializeJcMatrices(entitiesById.Values);
                attempts++;
            }
            else
            {
                Console.WriteLine(
                    $"Successfully scheduled requirement for S=[{string.Join(",", currentReq.EntitiesList)}]. Results: {currentReq.AssignedTimeslotList.Count} occurrences scheduled.");
            }
        }

        if (attempts >= maxAttempts)
        {
            Console.WriteLine(
                $"Scheduling failed after {maxAttempts} attempts. Could not schedule all requirements.");
            return false;
        }

        Console.WriteLine("Abstract schedule solved. Projecting onto calendar...");
        var dayIndexToIdMap = timetable.Days.OrderBy(d => (int)Enum.Parse<DayOfWeek>(d.Name, true))
            .Select((day, index) => new { day.Id, Index = index })
            .ToDictionary(x => x.Index, x => x.Id);

        var periodIndexToIdMap = sortedPeriods
            .Select((period, index) => new { period.Id, Index = index })
            .ToDictionary(x => x.Index, x => x.Id);

        var abstractSchedules = new List<AbstractClassSchedule>();
        foreach (var requirement in currentDocument)
        {
            var classEntitySchedulingId = requirement.EntitiesList.FirstOrDefault(id => id >= ClassEntityIdOffset);
            if (classEntitySchedulingId == 0) continue;
            var classEntityId = classEntitySchedulingId - ClassEntityIdOffset;

            foreach (var resultPair in requirement.AssignedTimeslotList)
                if (dayIndexToIdMap.TryGetValue(resultPair.Day, out var dayId) &&
                    periodIndexToIdMap.TryGetValue(resultPair.Period, out var periodId))
                    abstractSchedules.Add(new AbstractClassSchedule(classEntityId, dayId, periodId));
        }

        var concreteOccurrences = calendarizationService.ProjectSchedule(timetable, abstractSchedules);
        await classOccurrenceRepository.AddRangeAsync(concreteOccurrences);

        Console.WriteLine(
            $"Finished schedule generation for timetable ID: {timetableId}. Created {concreteOccurrences.Count} concrete occurrences.");
        return true;

        void InitializeJcMatrices(IEnumerable<SchedulingEntity> entitiesToReset)
        {
            foreach (var entity in entitiesToReset)
                entity.AvailabilityMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
        }
    }
}
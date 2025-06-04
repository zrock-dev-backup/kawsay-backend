using Application.DTOs;
using Application.Features.Scheduling.Algorithm;
using Application.Features.Scheduling.Models;
using Application.Features.Scheduling.Utils;
using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;

namespace Application.Features.Scheduling;

public class SchedulingService(
    ITimetableRepository timetableRepository,
    IClassRepository classRepository,
    ITeacherRepository teacherRepository,
    IClassOccurrenceRepository classOccurrenceRepository
)
{
    public const int ClassEntityIdOffset = 10000;

    public async Task<bool> GenerateScheduleAsync(int timetableId)
    {
        Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");

        // Gather data from database
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
        if (!allTeachers.Any()) throw new ArgumentException($"No teachers found in database.");

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
        const int maxAttempts = 100;
        var currentDocument = new LinkedList<SchedulingRequirementLine>(requirementDocument);
        var enumerator = currentDocument.GetEnumerator();
        InitializeJcMatrices();

        while (enumerator.MoveNext() && attempts < maxAttempts)
        {
            var currentReq = enumerator.Current;
            if (!YuleAlgorithm.Handler(currentReq, allSchedulingEntities, timetable.Days.Count,
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
                    $"Successfully scheduled requirement for S=[{string.Join(",", currentReq.EntitiesList)}]. Results: {currentReq.AssignedTimeslotList.Count} occurrences scheduled.");
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

        var newOccurrences = new List<ClassOccurrenceEntity>();
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
            foreach (var resultPair in requirement.AssignedTimeslotList)
            {
                var pair = mapHelper.GetId(resultPair);
                var dayIndex = pair.Day;
                var periodIndex = pair.Period;
                newOccurrences.Add(new ClassOccurrenceEntity
                {
                    ClassId = classEntityId,
                    DayId = dayIndex,
                    StartPeriodId = periodIndex
                });
            }

            Console.WriteLine(
                $"Created {requirement.AssignedTimeslotList.Count} new occurrences for Class Entity ID {classEntityId}.");
        }

        classOccurrenceRepository.AddRangeAsync(newOccurrences);
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
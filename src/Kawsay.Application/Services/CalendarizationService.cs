using Application.Models;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Projects an abstract weekly schedule onto a concrete calendar based on a date range.
/// </summary>
public class CalendarizationService
{
    /// <summary>
    /// Generates a list of concrete, dated class occurrences from an abstract template.
    /// </summary>
    /// <param name="timetable">The timetable containing the date range and day definitions.</param>
    /// <param name="abstractSchedules">The abstract weekly schedule from the solver.</param>
    /// <returns>A list of ClassOccurrenceEntity objects, each with a specific date.</returns>
    public virtual List<ClassOccurrenceEntity> ProjectSchedule(
        TimetableEntity timetable,
        List<AbstractClassSchedule> abstractSchedules)
    {
        var concreteOccurrences = new List<ClassOccurrenceEntity>();

        if (timetable.StartDate > timetable.EndDate || abstractSchedules.Count == 0)
        {
            return concreteOccurrences;
        }

        // Create a lookup from DayOfWeek (e.g., Monday) to the database DayId.
        var dayOfWeekToDayIdMap = timetable.Days
            .ToDictionary(day => Enum.Parse<DayOfWeek>(day.Name, true), day => day.Id);

        // Group abstract schedules by DayId for efficient lookup during the date iteration.
        var schedulesByDayId = abstractSchedules
            .GroupBy(s => s.DayId)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (var date = timetable.StartDate; date <= timetable.EndDate; date = date.AddDays(1))
        {
            // Check if the current day of the week is part of the timetable's schedule.
            if (dayOfWeekToDayIdMap.TryGetValue(date.DayOfWeek, out var dayId))
            {
                // Check if there are any abstract schedules for this day.
                if (schedulesByDayId.TryGetValue(dayId, out var dailySchedules))
                {
                    foreach (var abstractSchedule in dailySchedules)
                    {
                        var concreteOccurrence = new ClassOccurrenceEntity
                        {
                            ClassId = abstractSchedule.ClassId,
                            Date = date,
                            StartPeriodId = abstractSchedule.StartPeriodId
                        };
                        concreteOccurrences.Add(concreteOccurrence);
                    }
                }
            }
        }

        return concreteOccurrences;
    }
}

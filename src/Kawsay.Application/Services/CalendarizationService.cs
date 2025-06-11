using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class CalendarizationService
{
    public virtual List<ClassOccurrenceEntity> ProjectSchedule(
        TimetableEntity timetable,
        List<AbstractClassSchedule> abstractSchedules)
    {
        var concreteOccurrences = new List<ClassOccurrenceEntity>();

        if (timetable.StartDate > timetable.EndDate || abstractSchedules.Count == 0)
        {
            return concreteOccurrences;
        }

        var dayOfWeekToDayIdMap = timetable.Days
            .ToDictionary(day => Enum.Parse<DayOfWeek>(day.Name, true), day => day.Id);

        var schedulesByDayId = abstractSchedules
            .GroupBy(s => s.DayId)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (var date = timetable.StartDate; date <= timetable.EndDate; date = date.AddDays(1))
        {
            if (dayOfWeekToDayIdMap.TryGetValue(date.DayOfWeek, out var dayId))
            {
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

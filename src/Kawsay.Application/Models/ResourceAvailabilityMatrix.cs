using Domain.Entities;
using Domain.Interfaces;

namespace Application.Models;

public class ResourceAvailabilityMatrix
{
    private readonly int[,] _matrix;
    private readonly Dictionary<DayOfWeek, int> _dayMap;
    private readonly Dictionary<int, int> _periodMap;
    private readonly int _periodCount;

    public ResourceAvailabilityMatrix(TimetableEntity timetable, IEnumerable<ISchedulable> scheduledItems)
    {
        var sortedDays = timetable.Days
            .OrderBy(d => (int)(Enum.Parse<DayOfWeek>(d.Name, true) + 6) % 7)
            .ToList();
        
        var sortedPeriods = timetable.Periods.OrderBy(p => p.Start).ToList();

        _dayMap = sortedDays.Select((day, index) => new { DayOfWeek = Enum.Parse<DayOfWeek>(day.Name, true), Index = index })
            .ToDictionary(x => x.DayOfWeek, x => x.Index);
        
        _periodMap = sortedPeriods.Select((period, index) => new { period.Id, Index = index })
            .ToDictionary(x => x.Id, x => x.Index);

        _periodCount = sortedPeriods.Count;
        _matrix = new int[sortedDays.Count, _periodCount];
        
        PopulateMatrix(scheduledItems);
    }

    private void PopulateMatrix(IEnumerable<ISchedulable> scheduledItems)
    {
        foreach (var item in scheduledItems)
        foreach (var occurrence in item.ClassOccurrences)
        {
            if (!_dayMap.TryGetValue(occurrence.Date.DayOfWeek, out var dayIndex)) continue;
            if (!_periodMap.TryGetValue(occurrence.StartPeriodId, out var startPeriodIndex)) continue;
            
            for (var i = 0; i < item.Length; i++)
            {
                var currentPeriodIndex = startPeriodIndex + i;
                if (currentPeriodIndex < _periodCount)
                {
                    _matrix[dayIndex, currentPeriodIndex] = 1; // Mark as busy
                }
            }
        }
    }

    public bool HasClash(ISchedulable itemToCheck)
    {
        foreach (var occurrence in itemToCheck.ClassOccurrences)
        {
            if (!_dayMap.TryGetValue(occurrence.Date.DayOfWeek, out var dayIndex)) continue;
            if (!_periodMap.TryGetValue(occurrence.StartPeriodId, out var startPeriodIndex)) continue;

            for (var i = 0; i < itemToCheck.Length; i++)
            {
                var currentPeriodIndex = startPeriodIndex + i;
                if (currentPeriodIndex >= _periodCount) continue;
                if (_matrix[dayIndex, currentPeriodIndex] == 1) return true;
            }
        }
        return false;
    }
}

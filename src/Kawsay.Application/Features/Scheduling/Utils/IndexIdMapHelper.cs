using Application.Features.Scheduling.Models;

namespace Application.Features.Scheduling.Utils;

public class IndexIdMapHelper(int amntDays, int amntPeriods, Dictionary<int, List<int>> timetableIds)
{
    private readonly Dictionary<TimetablePair, TimetablePair> _idMap = PopulateMap(amntDays, amntPeriods, timetableIds) ?? new Dictionary<TimetablePair, TimetablePair>();

    private static Dictionary<TimetablePair, TimetablePair>? PopulateMap(int amntDays, int amntPeriods, Dictionary<int, List<int>> timetableIds)
    {
        var map = new Dictionary<TimetablePair, TimetablePair>();
        if (timetableIds.Count != amntDays)
        {
            Console.WriteLine("Not enough timetable day ids");
            return null;
        }

        var dayIndex = 0;
        foreach (var keyValuePair in timetableIds)
        {
            if (keyValuePair.Value.Count != amntPeriods)
            {
                Console.WriteLine("Invalid amnt of periods for day");
                return null;
            }

            var periodIndex = 0;
            foreach (var periodId in keyValuePair.Value)
            {
                map.Add(new TimetablePair(dayIndex, periodIndex), new TimetablePair(keyValuePair.Key, periodId));
                periodIndex++;
            }
            dayIndex++;
        }

        TransposeMap();
        return map;

        void TransposeMap()
        {
            foreach (var keyValuePair in map.ToList())
            {
                map.Add(keyValuePair.Value, keyValuePair.Key);
            }
        }
    }

    public TimetablePair GetId(TimetablePair key)
    {
        return _idMap[key];
    }
}
namespace kawsay.Scheduling.Utils;

public class IndexIdMapHelper(int amntDays, int amntPeriods, Dictionary<int, List<int>> timetableIds)
{
    private readonly Dictionary<Pair, Pair> _idMap = PopulateMap(amntDays, amntPeriods, timetableIds) ?? new Dictionary<Pair, Pair>();

    private static Dictionary<Pair, Pair>? PopulateMap(int amntDays, int amntPeriods, Dictionary<int, List<int>> timetableIds)
    {
        var map = new Dictionary<Pair, Pair>();
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
                map.Add(new Pair(dayIndex, periodIndex), new Pair(keyValuePair.Key, periodId));
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

    public Pair GetId(Pair key)
    {
        return _idMap[key];
    }
}
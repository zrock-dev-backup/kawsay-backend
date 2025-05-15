namespace Application.Features.Scheduling.Models;

public class ScheduledTimeslotResult
{
    public readonly Dictionary<int, List<int>> TimeslotDict = new();
    private int _index;

    public void Add(int dayIndex, int periodIndex)
    {
        TimeslotDict.Add(_index++, new List<int> { dayIndex, periodIndex });
    }

    public void Clear()
    {
        TimeslotDict.Clear();
        _index = 0;
    }
}
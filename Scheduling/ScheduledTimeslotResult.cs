namespace KawsayApiMockup.Scheduling;

public class ScheduledTimeslotResult
{
    public readonly Dictionary<int, List<int>> R = new();
    private int occurrenceIndex;

    public void Add(int dayIndex, int periodIndex)
    {
        R.Add(occurrenceIndex++, new List<int> { dayIndex, periodIndex });
    }

    public void Clear()
    {
        R.Clear();
        occurrenceIndex = 0;
    }
}
namespace Application.Features.Scheduling.Models;

public class SchedulingRequirementLine(
    int frequency,
    int length,
    List<int> entitiesId,
    int numDays,
    int numPeriods)
{
    public List<int> EntitiesList { get; } = entitiesId;
    public int Frequency { get; } = frequency;
    public List<Pair> AssignedTimeslotList { get; } = new();
    public List<int> PeriodPreferenceList { get; } = Enumerable.Repeat(1, numPeriods).ToList();
    public int Length { get; } = length;
    public SchedulingMatrix AvailabilityMatrix { get; set; } = new(numDays, numPeriods);


    public void SetZ(int periodIndex, int value)
    {
        if (periodIndex >= 0 && periodIndex < PeriodPreferenceList.Count)
            PeriodPreferenceList[periodIndex] = value;
        else
            Console.WriteLine(
                $"Warning: Attempted to set Z[{periodIndex}] out of bounds. Z size: {PeriodPreferenceList.Count}.");
    }


    public void SetZRange(int startPeriodIndex, int endPeriodIndex, int value)
    {
        var inclusiveEndIndex = endPeriodIndex + 1;
        for (var i = startPeriodIndex; i < inclusiveEndIndex; i++)
            if (i >= 0 && i < PeriodPreferenceList.Count)
                PeriodPreferenceList[i] = value;
            else
                Console.WriteLine(
                    $"Warning: Attempted to set Z range [{startPeriodIndex}-{endPeriodIndex}] out of bounds. Index {i} out of {PeriodPreferenceList.Count}.");
    }
}
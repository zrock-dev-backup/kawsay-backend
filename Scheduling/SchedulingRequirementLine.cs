namespace kawsay.Scheduling;

public abstract class SchedulingRequirementLine
{
    public SchedulingRequirementLine(int q, int length, List<int> s, int numDays, int numPeriods)
    {
        this.q = q;
        this.length = length;
        S = s ?? new List<int>();
        R = new ScheduledTimeslotResult();
        E = new SchedulingMatrix(numDays, numPeriods);
        Z = Enumerable.Repeat(1, numPeriods).ToList();
    }

    public List<int> S { get; set; }
    public int q { get; set; }
    public ScheduledTimeslotResult R { get; set; }
    public List<int> Z { get; set; }
    public int length { get; set; }
    public SchedulingMatrix E { get; set; }


    protected void SetZ(int periodIndex, int value)
    {
        if (periodIndex >= 0 && periodIndex < Z.Count)
            Z[periodIndex] = value;
        else
            Console.WriteLine($"Warning: Attempted to set Z[{periodIndex}] out of bounds. Z size: {Z.Count}.");
    }


    protected void SetZRange(int startPeriodIndex, int endPeriodIndex, int value)
    {
        var inclusiveEndIndex = endPeriodIndex + 1;
        for (var i = startPeriodIndex; i < inclusiveEndIndex; i++)
            if (i >= 0 && i < Z.Count)
                Z[i] = value;
            else
                Console.WriteLine(
                    $"Warning: Attempted to set Z range [{startPeriodIndex}-{endPeriodIndex}] out of bounds. Index {i} out of {Z.Count}.");
    }
}
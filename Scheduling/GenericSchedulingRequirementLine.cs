namespace kawsay.Scheduling;

public class GenericSchedulingRequirementLine : SchedulingRequirementLine
{
    public GenericSchedulingRequirementLine(int q, int length, List<int> s, int numDays, int numPeriods)
        : base(q, length, s, numDays, numPeriods)
    {
    }
}
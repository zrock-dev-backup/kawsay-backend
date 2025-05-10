namespace kawsay.Scheduling;

public class SchedulingEntity(int id, string name, int numDays, int numPeriods)
{
    public readonly int Id = id;
    public readonly string Name = name;
    public SchedulingMatrix AvailabilityMatrix = new(numDays, numPeriods);


    public override string ToString()
    {
        return Name;
    }
}
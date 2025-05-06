namespace kawsay.Scheduling;

public class SchedulingEntity
{
    public readonly int Id;
    public readonly string Name;
    public SchedulingMatrix jC;


    public SchedulingEntity(int id, string name, int numDays, int numPeriods)
    {
        Id = id;
        Name = name;
        jC = new SchedulingMatrix(numDays, numPeriods);
    }

    public override string ToString()
    {
        return Name;
    }
}
namespace Application.Features.Scheduling.Models;

public class TimetablePair(int day, int period)
{
    public int Day { get; } = day;
    public int Period { get; } = period;

    public override bool Equals(object? obj)
    {
        return obj is TimetablePair other && Equals(other);
    }

    private bool Equals(TimetablePair other)
    {
        return Day == other.Day && Period == other.Period;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Day, Period);
    }
}
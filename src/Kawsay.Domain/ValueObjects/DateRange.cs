namespace Kawsay.Domain.ValueObjects;

public record DateRange
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public DateRange(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date.", nameof(startDate));
        }
        StartDate = startDate;
        EndDate = endDate;
    }
}

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
    public List<TimetablePair> AssignedTimeslotList { get; } = new();
    public SchedulingMatrix PeriodPreferenceMatrix { get; set; } = InitMatrix(numDays, numPeriods);
    public int Length { get; } = length;
    public SchedulingMatrix AvailabilityMatrix { get; set; } = new(numDays, numPeriods);

    // TODO: wouldn't it be better if SchedulingMAtrix already starts with ones?
    private static SchedulingMatrix InitMatrix(int numDays, int numPeriods)
    {
        var matrix = new SchedulingMatrix(numDays, numPeriods);
        for (var i = 0; i < numDays; i++)
        {
            for (var j = 0; j < numPeriods; j++)
            {
                matrix.Set(i, j, 1); // 1 = Not preferred by default
            }
        }

        return matrix;
    }
}
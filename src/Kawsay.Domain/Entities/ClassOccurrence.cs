using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ClassOccurrence
{
    [Key] public int Id { get; set; }
    
    public int ClassId { get; set; }
    public ClassEntity Class { get; set; } = default!;
    
    public int DayId { get; set; }
    public TimetableDayEntity DayEntity { get; set; } = default!;
    
    public int StartPeriodId { get; set; }
    public TimetablePeriodEntity StartPeriod { get; set; } = default!;
}
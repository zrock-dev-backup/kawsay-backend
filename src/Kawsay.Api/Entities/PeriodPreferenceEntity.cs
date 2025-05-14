using System.ComponentModel.DataAnnotations;

namespace Api.Entities;

public class PeriodPreferenceEntity
{
    [Key] public int Id { get; set; }
    
    public int ClassId { get; set; }
    public ClassEntity Class { get; set; } = default!;
    
    public int StartPeriodId { get; set; }
    public TimetablePeriodEntity StartPeriod { get; set; } = default!;
}
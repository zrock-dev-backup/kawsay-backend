using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ClassOccurrenceEntity
{
    [Key] public int Id { get; set; }
    
    public int ClassId { get; set; }
    public ClassEntity Class { get; set; } = default!;
    [Required] public DateOnly Date { get; set; }
    
    public int StartPeriodId { get; set; }
    public TimetablePeriodEntity StartPeriod { get; set; } = default!;
}
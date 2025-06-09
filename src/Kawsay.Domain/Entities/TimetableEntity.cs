using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TimetableEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
    public ICollection<TimetableDayEntity> Days { get; set; } = new List<TimetableDayEntity>();
    public ICollection<TimetablePeriodEntity> Periods { get; set; } = new List<TimetablePeriodEntity>();
    public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
}
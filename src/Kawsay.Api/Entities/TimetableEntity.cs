using System.ComponentModel.DataAnnotations;

namespace Api.Entities;

public class TimetableEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    public ICollection<TimetableDayEntity> Days { get; set; } = new List<TimetableDayEntity>();
    public ICollection<TimetablePeriodEntity> Periods { get; set; } = new List<TimetablePeriodEntity>();
    public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
}
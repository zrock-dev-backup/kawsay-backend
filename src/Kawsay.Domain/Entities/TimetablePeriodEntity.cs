using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TimetablePeriodEntity : IComparable
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(5)] public string Start { get; set; } = string.Empty;
    [Required] [MaxLength(5)] public string End { get; set; } = string.Empty;
    
    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
    
    public ICollection<PeriodPreferenceEntity> PeriodPreferences { get; set; } = new List<PeriodPreferenceEntity>();
    public ICollection<ClassOccurrence> Occurrences { get; set; } = new List<ClassOccurrence>();


    public int CompareTo(object? obj)
    {
        if (obj is not TimetablePeriodEntity other)
            throw new ArgumentException("Object is not a TimetablePeriodEntity");

        return Id.CompareTo(other.Id);
    }
}
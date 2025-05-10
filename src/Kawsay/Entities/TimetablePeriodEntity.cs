using System.ComponentModel.DataAnnotations;

namespace kawsay.Entities;

public class TimetablePeriodEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(5)] public string Start { get; set; } = string.Empty;
    [Required] [MaxLength(5)] public string End { get; set; } = string.Empty;
    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
    public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
}
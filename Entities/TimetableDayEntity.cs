using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities;

public class TimetableDayEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(20)] public string Name { get; set; } = string.Empty;
    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
    public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
}
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class HolidayEntity
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    [Required] public DateOnly Date { get; set; }

    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
}
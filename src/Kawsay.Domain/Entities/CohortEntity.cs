using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class CohortEntity
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;

    public ICollection<StudentGroupEntity> StudentGroups { get; set; } = new List<StudentGroupEntity>();
}
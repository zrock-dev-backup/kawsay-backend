using System.ComponentModel.DataAnnotations;

namespace kawsay.Entities;

public class ClassEntity
{
    [Key] public int Id { get; set; }
    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = default!;
    public int? TeacherId { get; set; }
    public TeacherEntity? Teacher { get; set; }
    [Required] public int RequiredOccurrenceCount { get; init; }
    [Required] public int OccurrenceLength { get; init; }
    public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
}
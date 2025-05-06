using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities;

public class ClassEntity
{
    [Key] public int Id { get; set; }
    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = default!;
    public int? TeacherId { get; set; }
    public TeacherEntity? Teacher { get; set; }
    [Required] public int RequiredOccurrenceCount { get; set; }
    [Required] public int OccurrenceLength { get; set; }
    public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
}
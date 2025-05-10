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
    
    [Required] public int Frequency { get; init; }
    [Required] public int Length { get; init; }
    
    public ICollection<PeriodPreference> PeriodPreferences { get; set; } = new List<PeriodPreference>();
}
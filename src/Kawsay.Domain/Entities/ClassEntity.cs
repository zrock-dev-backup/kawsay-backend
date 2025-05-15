using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ClassEntity
{
    [Key] public int Id { get; set; }
    
    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
    
    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = default!;
    
    public int TeacherId { get; set; }
    public TeacherEntity Teacher { get; set; } = default!;
    
    [Required] public int Frequency { get; init; }
    [Required] public int Length { get; init; }
    
    public ICollection<PeriodPreferenceEntity> PeriodPreferences { get; set; } = new List<PeriodPreferenceEntity>();
    public ICollection<ClassOccurrenceEntity> ClassOccurrences { get; set; } = new List<ClassOccurrenceEntity>();
}
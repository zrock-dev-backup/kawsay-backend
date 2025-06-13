using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities;

public class ClassEntity : ISchedulable
{
    [Key] public int Id { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;

    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = default!;

    public int? TeacherId { get; set; }
    public TeacherEntity? Teacher { get; set; } = default!;

    [Required] public int Frequency { get; set; }
    [Required] public int Capacity { get; set; }
    [Required] public int Length { get; set; }

    [Required] public ClassType ClassType { get; set; }

    public int? StudentGroupId { get; set; }
    public StudentGroupEntity? StudentGroup { get; set; }

    public int? SectionId { get; set; }
    public SectionEntity? Section { get; set; }

    public ICollection<PeriodPreferenceEntity> PeriodPreferences { get; set; } = new List<PeriodPreferenceEntity>();
    public ICollection<ClassOccurrenceEntity> ClassOccurrences { get; set; } = new List<ClassOccurrenceEntity>();
    public ICollection<EnrollmentEntity> Enrollments { get; set; } = new List<EnrollmentEntity>();
}
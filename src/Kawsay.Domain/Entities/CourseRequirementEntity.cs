using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kawsay.Domain.Enums;
using Kawsay.Domain.ValueObjects;

namespace Domain.Entities;

public class CourseRequirementEntity
{
    [Key] public int Id { get; set; }

    [Required] public int TimetableId { get; set; }
    [ForeignKey(nameof(TimetableId))] public TimetableEntity Timetable { get; set; } = default!;

    [Required] public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))] public CourseEntity Course { get; set; } = default!;

    public int? StudentGroupId { get; set; }
    [ForeignKey(nameof(StudentGroupId))] public StudentGroupEntity? StudentGroup { get; set; }

    public int? SectionId { get; set; }
    [ForeignKey(nameof(SectionId))] public SectionEntity? Section { get; set; }

    public int? PreferredTeacherId { get; set; }

    [ForeignKey(nameof(PreferredTeacherId))]
    public TeacherEntity? PreferredTeacher { get; set; }

    [Required] public CourseRequirementPriority Priority { get; set; }

    [Required] public DateRange EffectiveDateRange { get; set; } = default!;

    [Required] [Range(1, 10)] public int DurationInPeriods { get; set; }

    [Required] [Range(1, 7)] public int FrequencyPerWeek { get; set; }

    public int? RequiredCapacity { get; set; }

    [Required] public CourseRequirementStatus Status { get; set; } = CourseRequirementStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SoftSchedulingPreferenceEntity> SoftPreferences { get; set; } =
        new List<SoftSchedulingPreferenceEntity>();
}
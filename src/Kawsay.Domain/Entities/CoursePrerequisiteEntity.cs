using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class CoursePrerequisiteEntity
{
    [Key] public int Id { get; set; }

    [Required] public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))] public CourseEntity Course { get; set; } = default!;

    [Required] public int PrerequisiteCourseId { get; set; }

    [ForeignKey(nameof(PrerequisiteCourseId))]
    public CourseEntity PrerequisiteCourse { get; set; } = default!;
}
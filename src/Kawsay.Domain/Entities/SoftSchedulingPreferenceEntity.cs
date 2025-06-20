using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class SoftSchedulingPreferenceEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CourseRequirementId { get; set; }
    [ForeignKey(nameof(CourseRequirementId))]
    public CourseRequirementEntity CourseRequirement { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public string PreferenceType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    public int? Weight { get; set; }
}

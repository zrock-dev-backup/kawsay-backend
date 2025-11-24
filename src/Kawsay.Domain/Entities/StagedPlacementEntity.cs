using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class StagedPlacementEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CourseRequirementId { get; set; }
    [ForeignKey(nameof(CourseRequirementId))]
    public CourseRequirementEntity CourseRequirement { get; set; } = default!;

    [Required]
    public int DayId { get; set; }
    [ForeignKey(nameof(DayId))]
    public TimetableDayEntity Day { get; set; } = default!;

    [Required]
    public int StartPeriodId { get; set; }
    [ForeignKey(nameof(StartPeriodId))]
    public TimetablePeriodEntity StartPeriod { get; set; } = default!;

    public int Length { get; set; } // Denormalized from Requirement for easier querying
}

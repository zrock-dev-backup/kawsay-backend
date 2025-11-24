using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class TimetableAssignmentEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TimetableId { get; set; }
    [ForeignKey(nameof(TimetableId))]
    public TimetableEntity Timetable { get; set; } = default!;

    [Required]
    public int TeacherId { get; set; }
    [ForeignKey(nameof(TeacherId))]
    public TeacherEntity Teacher { get; set; } = default!;

    [Required]
    public int StartWeek { get; set; }

    [Required]
    public int EndWeek { get; set; }

    [Required]
    public int MaximumWorkload { get; set; }

    [Required]
    public WorkloadUnit WorkloadUnit { get; set; }
}

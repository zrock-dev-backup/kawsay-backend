using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class StudentIssueEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }
    [ForeignKey(nameof(StudentId))]
    public StudentEntity Student { get; set; } = default!;

    [Required]
    public int TimetableId { get; set; } // Issues are scoped to a timetable context

    [Required]
    [MaxLength(50)]
    public string IssueType { get; set; } = string.Empty; // "Prerequisite", "TimeClash", etc.

    [Required]
    public string Details { get; set; } = string.Empty; // JSON or text description

    public bool IsResolved { get; set; }
}

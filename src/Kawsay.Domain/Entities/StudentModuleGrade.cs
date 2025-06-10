using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class StudentModuleGrade
{
    [Key] public int Id { get; set; }

    [Required] public int StudentId { get; set; }
    public StudentEntity Student { get; set; } = default!;

    [Required] public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = default!;

    [Required] public int TimetableId { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;

    [Required]
    [Column(TypeName = "decimal(5, 2)")]
    public decimal GradeValue { get; set; }

    public bool IsPassing { get; set; }
}
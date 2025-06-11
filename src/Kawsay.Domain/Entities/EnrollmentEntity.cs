using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class EnrollmentEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public StudentEntity Student { get; set; } = default!;

    [Required]
    public int ClassId { get; set; }

    [ForeignKey(nameof(ClassId))]
    public ClassEntity Class { get; set; } = default!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}

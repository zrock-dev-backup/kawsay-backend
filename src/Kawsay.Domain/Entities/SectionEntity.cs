using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class SectionEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int StudentGroupId { get; set; }
    public StudentGroupEntity StudentGroup { get; set; } = default!;

    public ICollection<StudentEntity> Students { get; set; } = new List<StudentEntity>();

    public ICollection<ClassEntity> LabClasses { get; set; } = new List<ClassEntity>();
}

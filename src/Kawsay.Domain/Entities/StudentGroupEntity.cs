using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class StudentGroupEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int CohortId { get; set; }
    public CohortEntity Cohort { get; set; } = default!;

    public ICollection<SectionEntity> Sections { get; set; } = new List<SectionEntity>();

    public ICollection<ClassEntity> Masterclasses { get; set; } = new List<ClassEntity>();
}

using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TeacherEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(50)] public string Type { get; set; } = string.Empty;
    public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
}
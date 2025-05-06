using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities;

public class CourseEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(20)] public string Code { get; set; } = string.Empty;
    public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
}
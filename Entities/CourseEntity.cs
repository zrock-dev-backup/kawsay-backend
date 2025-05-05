// Entities/CourseEntity.cs
using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities
{
    public class CourseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Example validation attribute
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)] // Example validation attribute
        public string Code { get; set; } = string.Empty;

        // Navigation property for related classes (optional for this version, but good practice)
        public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
    }
}

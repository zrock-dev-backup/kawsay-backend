// Entities/TeacherEntity.cs
using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities
{
    public class TeacherEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Example validation attribute
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)] // Example validation attribute
        public string Type { get; set; } = string.Empty; // "Professor" or "Faculty Practitioner"

        // Navigation property for related classes (optional)
        public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
    }
}

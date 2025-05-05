// Entities/TimetableEntity.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KawsayApiMockup.Entities
{
    public class TimetableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Example validation attribute
        public string Name { get; set; } = string.Empty;

        // Navigation properties for related days, periods, and classes
        public ICollection<TimetableDayEntity> Days { get; set; } = new List<TimetableDayEntity>();
        public ICollection<TimetablePeriodEntity> Periods { get; set; } = new List<TimetablePeriodEntity>();
        public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
    }
}

// Entities/TimetableDayEntity.cs
using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities
{
    public class TimetableDayEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)] // e.g., "Monday"
        public string Name { get; set; } = string.Empty;

        // Foreign key to TimetableEntity
        public int TimetableId { get; set; }
        // Navigation property back to the parent timetable
        public TimetableEntity Timetable { get; set; } = default!; // 'default!' to satisfy non-nullable property with EF Core

        // Navigation property for occurrences on this day (optional)
        public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
    }
}

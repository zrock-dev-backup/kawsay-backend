// Entities/TimetablePeriodEntity.cs
using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities
{
    public class TimetablePeriodEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(5)] // HH:mm format
        public string Start { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)] // HH:mm format
        public string End { get; set; } = string.Empty;

        // Foreign key to TimetableEntity
        public int TimetableId { get; set; }
        // Navigation property back to the parent timetable
        public TimetableEntity Timetable { get; set; } = default!;

        // Navigation property for occurrences starting at this period (optional)
        public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
    }
}

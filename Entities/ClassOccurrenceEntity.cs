// Entities/ClassOccurrenceEntity.cs
using System.ComponentModel.DataAnnotations;

namespace KawsayApiMockup.Entities
{
    public class ClassOccurrenceEntity
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to ClassEntity
        public int ClassId { get; set; }
        // Navigation property back to the parent class
        public ClassEntity Class { get; set; } = default!;

        // Foreign key to TimetableDayEntity
        public int DayId { get; set; }
        // Navigation property to the specific day
        public TimetableDayEntity Day { get; set; } = default!;

        // Foreign key to TimetablePeriodEntity (represents the STARTING period)
        public int StartPeriodId { get; set; }
        // Navigation property to the starting period
        public TimetablePeriodEntity StartPeriod { get; set; } = default!; // Renamed from 'Period' to 'StartPeriod' for clarity

        [Required]
        public int Length { get; set; } // Number of consecutive periods
    }
}

// Entities/ClassEntity.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KawsayApiMockup.Entities // Ensure this namespace is correct for your project
{
    public class ClassEntity
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to TimetableEntity
        public int TimetableId { get; set; }
        // Navigation property to the parent timetable
        public TimetableEntity Timetable { get; set; } = default!;

        // Foreign key to CourseEntity
        public int CourseId { get; set; }
        // Navigation property to the associated course
        public CourseEntity Course { get; set; } = default!;

        // Foreign key to TeacherEntity (nullable)
        public int? TeacherId { get; set; }
        // Navigation property to the associated teacher (nullable)
        public TeacherEntity? Teacher { get; set; }

        // --- Properties for Scheduling Algorithm Input ---
        [Required] // These properties are required for the algorithm to know what to schedule
        public int RequiredOccurrenceCount { get; set; } // How many occurrences are needed for this class (q)

        [Required] // These properties are required for the algorithm to know what to schedule
        public int OccurrenceLength { get; set; } // How many periods long each occurrence should be (length)
        // ------------------------------------------------------


        // Navigation property for scheduled occurrences (These are the *results* of scheduling)
        public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
    }
}

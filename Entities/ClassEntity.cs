// Entities/ClassEntity.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KawsayApiMockup.Entities
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

        // Navigation property for scheduled occurrences
        public ICollection<ClassOccurrenceEntity> Occurrences { get; set; } = new List<ClassOccurrenceEntity>();
    }
}

// DTOs/ClassDTOs.cs
using System.Collections.Generic;

namespace KawsayApiMockup.DTOs
{
    // Represents a single scheduled block within a class
    public class ClassOccurrence
    {
        public int Id { get; set; } // ID is always present in fetched/updated occurrences
        public int DayId { get; set; } // References TimetableDay.id
        public int StartPeriodId { get; set; } // References TimetablePeriod.id
        public int Length { get; set; } // Number of consecutive periods
    }

    // Represents a class (GET response for /classes and /class/{id}, POST/PUT response for /class)
    public class Class
    {
        public int Id { get; set; }
        public int TimetableId { get; set; } // References TimetableStructure.id
        public Course Course { get; set; } = new Course(); // Embedded Course object
        public Teacher? Teacher { get; set; } // Embedded Teacher object (nullable)
        public List<ClassOccurrence> Occurrences { get; set; } = new List<ClassOccurrence>();
    }

    // Represents a single scheduled block in the POST /kawsay/class request
    public class CreateClassOccurrenceDto
    {
        public int DayId { get; set; }
        public int StartPeriodId { get; set; }
        public int Length { get; set; }
    }

    // Represents the request body for POST /kawsay/class
    public class CreateClassRequest
    {
        public int TimetableId { get; set; }
        public int CourseId { get; set; } // References Course.id
        public int? TeacherId { get; set; } // References Teacher.id (nullable)
        public List<CreateClassOccurrenceDto> Occurrences { get; set; } = new List<CreateClassOccurrenceDto>();
    }

    // Represents the request body for PUT /kawsay/class/{id} (Added for API completeness, not used by current frontend)
     public class UpdateClassRequest
    {
        public int Id { get; set; } // Should match URL param
        public int TimetableId { get; set; } // Should match the class's current timetableId
        public int CourseId { get; set; } // References Course.id
        public int? TeacherId { get; set; } // References Teacher.id (nullable)
        public List<ClassOccurrence> Occurrences { get; set; } = new List<ClassOccurrence>(); // Include IDs for existing occurrences
    }
}

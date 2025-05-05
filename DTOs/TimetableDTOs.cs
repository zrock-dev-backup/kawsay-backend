// DTOs/TimetableDTOs.cs
using System.Collections.Generic;

namespace KawsayApiMockup.DTOs
{
    // Represents a day within a timetable structure
    public class TimetableDay
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Monday"
    }

    // Represents a time period within a timetable structure
    public class TimetablePeriod
    {
        public int Id { get; set; }
        public string Start { get; set; } = string.Empty; // HH:mm format
        public string End { get; set; } = string.Empty;   // HH:mm format
    }

    // Represents the full timetable structure (GET response for /timetable/{id} and POST response for /timetable)
    public class TimetableStructure
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<TimetableDay> Days { get; set; } = new List<TimetableDay>();
        public List<TimetablePeriod> Periods { get; set; } = new List<TimetablePeriod>();
    }

    // Represents the request body for POST /kawsay/timetable
    public class CreateTimetableRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Days { get; set; } = new List<string>(); // Array of day names (strings)
        public List<CreateTimetablePeriodDto> Periods { get; set; } = new List<CreateTimetablePeriodDto>(); // Array of periods for creation
    }

    // Helper DTO for periods in the CreateTimetableRequest
    public class CreateTimetablePeriodDto
    {
        public string Start { get; set; } = string.Empty; // HH:mm
        public string End { get; set; } = string.Empty;   // HH:mm
    }

    // Request body for PUT /kawsay/timetable/{id} (Added for API completeness, not used by current frontend)
    public class UpdateTimetableRequest
    {
        public int Id { get; set; } // Should match URL param
        public string Name { get; set; } = string.Empty;
        // Note: Updating days/periods might require a different structure depending on backend logic
        // For simplicity in mockup, assume full replacement or specific patch operations
        // This mockup will just update the name. A real API might require sending the full lists.
        // Let's match the creation request structure for simplicity in mockup PUT.
         public List<string> Days { get; set; } = new List<string>();
         public List<CreateTimetablePeriodDto> Periods { get; set; } = new List<CreateTimetablePeriodDto>();
    }
}

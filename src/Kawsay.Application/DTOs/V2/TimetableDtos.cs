using System.Text.Json.Serialization;

namespace Application.DTOs.V2;

// Using records for immutability and concise syntax
public record TimetableDayDto(int Id, string Name);
public record TimetablePeriodDto(int Id, string Start, string End);

public record TimetableStructureDto(
    int Id,
    string Name,
    string Status, // e.g., "Draft", "Published"
    string StartDate, // Format: "YYYY-MM-DD"
    string EndDate,   // Format: "YYYY-MM-DD"
    List<TimetableDayDto> Days,
    List<TimetablePeriodDto> Periods
);

public record CreateTimetablePeriodRequest(string Start, string End);

public record CreateTimetableRequest(
    string Name,
    string StartDate,
    string EndDate,
    List<string> Days,
    [property: JsonPropertyName("periods")] // Ensure exact match with frontend JSON
    List<CreateTimetablePeriodRequest> Periods
);

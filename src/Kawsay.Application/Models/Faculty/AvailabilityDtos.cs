// src/Kawsay.Application/Models/Faculty/AvailabilityDtos.cs
using System.Text.Json.Serialization;

namespace Application.Models.Faculty;

public record AvailabilityRangeDto(
    [property: JsonPropertyName("dayId")] int DayId, 
    [property: JsonPropertyName("periodStartId")] int PeriodStartId, 
    [property: JsonPropertyName("periodEndId")] int PeriodEndId);

public record AvailabilityRequestDto(
    [property: JsonPropertyName("status")] string Status, 
    [property: JsonPropertyName("range")] AvailabilityRangeDto Range);

public record AvailabilityPatchRequestDto(
    [property: JsonPropertyName("status")] string? Status, 
    [property: JsonPropertyName("range")] AvailabilityRangeDto? Range);

public record AvailabilityResponseDto(
    [property: JsonPropertyName("id")] int Id, 
    [property: JsonPropertyName("status")] string Status, 
    [property: JsonPropertyName("range")] AvailabilityRangeDto Range);

public record FacultyAvailabilityGroupResponseDto(
    [property: JsonPropertyName("facultyId")] int FacultyId, 
    [property: JsonPropertyName("availability")] List<AvailabilityResponseDto> Availability);

using System.ComponentModel.DataAnnotations;
using Kawsay.Domain.Enums;

namespace Application.DTOs;

public record SoftSchedulingPreferenceDto(
    [Required(ErrorMessage = "PreferenceType is required")]
    string PreferenceType,
    [Required(ErrorMessage = "Preference Value is required")]
    string Value,
    int? Weight
);

public record CourseRequirementBaseDto(
    [Required(ErrorMessage = "CourseId is required")]
    int CourseId,
    int? StudentGroupId,
    int? SectionId,
    int? PreferredTeacherId,
    [Required(ErrorMessage = "Priority level is required")]
    CourseRequirementPriority Priority,
    [Required(ErrorMessage = "Start date is required")]
    DateOnly StartDate,
    [Required(ErrorMessage = "End date is required")]
    DateOnly EndDate,
    [Required(ErrorMessage = "Duration in periods is required")]
    [Range(1, 10, ErrorMessage = "Duration must be between 1 and 10 periods")]
    int DurationInPeriods,
    [Required(ErrorMessage = "Weekly frequency is required")]
    [Range(1, 7, ErrorMessage = "Frequency must be between 1 and 7 times per week")]
    int FrequencyPerWeek,
    int? RequiredCapacity,
    List<SoftSchedulingPreferenceDto>? SoftPreferences
);

public record CreateCourseRequirementRequestDto : CourseRequirementBaseDto
{
    public CreateCourseRequirementRequestDto(
        int CourseId, int? StudentGroupId, int? SectionId, int? PreferredTeacherId,
        CourseRequirementPriority Priority, DateOnly StartDate, DateOnly EndDate, int DurationInPeriods,
        int FrequencyPerWeek, int? RequiredCapacity, List<SoftSchedulingPreferenceDto>? SoftPreferences)
        : base(CourseId, StudentGroupId, SectionId, PreferredTeacherId, Priority, StartDate, EndDate,
            DurationInPeriods, FrequencyPerWeek, RequiredCapacity, SoftPreferences)
    {
    }
}

public record UpdateCourseRequirementRequestDto : CourseRequirementBaseDto
{
    public UpdateCourseRequirementRequestDto(
        int CourseId, int? StudentGroupId, int? SectionId, int? PreferredTeacherId,
        CourseRequirementPriority Priority, DateOnly StartDate, DateOnly EndDate, int DurationInPeriods,
        int FrequencyPerWeek, int? RequiredCapacity, List<SoftSchedulingPreferenceDto>? SoftPreferences)
        : base(CourseId, StudentGroupId, SectionId, PreferredTeacherId, Priority, StartDate, EndDate,
            DurationInPeriods, FrequencyPerWeek, RequiredCapacity, SoftPreferences)
    {
    }
}

public record CourseRequirementDto(
    int Id,
    int TimetableId,
    CourseRequirementStatus Status, // Using Domain Enum
    DateTime CreatedAt,
    DateTime UpdatedAt,
    // Properties from CourseRequirementBaseDto
    int CourseId,
    int? StudentGroupId,
    int? SectionId,
    int? PreferredTeacherId,
    CourseRequirementPriority Priority,
    DateOnly StartDate,
    DateOnly EndDate,
    int DurationInPeriods,
    int FrequencyPerWeek,
    int? RequiredCapacity,
    List<SoftSchedulingPreferenceDto>? SoftPreferences
);

// --- DTOs for Available Slots ---
public record StagedPlacementSlotDetailDto(
    [Required(ErrorMessage = "Date is required")]
    DateOnly Date,
    [Required(ErrorMessage = "Time period ID is required")]
    int TimePeriodId,
    int? RoomId,
    int? TeacherId
);

public record AvailableSlotsRequestDto(
    [Required(ErrorMessage = "TransientStagedItems array is required for slot validation")]
    List<StagedPlacementSlotDetailDto>? TransientStagedItems
);

public enum AvailableSlotStatus
{
    Ideal,
    Viable,
    Conflict,
    Unavailable
}

public record AvailableSlotDto(
    [Required(ErrorMessage = "Slot key is required")]
    string SlotKey,
    [Required(ErrorMessage = "Date is required")]
    DateOnly Date,
    [Required(ErrorMessage = "Time period ID is required")]
    int TimePeriodId,
    int? RoomId,
    int? TeacherId,
    [Required(ErrorMessage = "Slot status is required")]
    AvailableSlotStatus Status,
    string? ConflictReason,
    double? Score
);

public record AvailableSlotsResponseDto(
    [Required(ErrorMessage = "Requirement ID is required")]
    int RequirementId,
    [Required(ErrorMessage = "Slots array is required")]
    List<AvailableSlotDto> Slots
);
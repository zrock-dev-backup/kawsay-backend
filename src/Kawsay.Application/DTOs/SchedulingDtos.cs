using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kawsay.Domain.Enums;

namespace Application.DTOs;

// --- Request DTOs ---

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
    List<SoftSchedulingPreferenceDto>? SoftPreferences,
    [Required]
    ClassTypeDto ClassType
);

public record CreateCourseRequirementRequestDto : CourseRequirementBaseDto
{
    [Required]
    public int TimetableId { get; init; }

    public CreateCourseRequirementRequestDto(
        int TimetableId,
        int CourseId, int? StudentGroupId, int? SectionId, int? PreferredTeacherId,
        CourseRequirementPriority Priority, DateOnly StartDate, DateOnly EndDate, int DurationInPeriods,
        int FrequencyPerWeek, int? RequiredCapacity, List<SoftSchedulingPreferenceDto>? SoftPreferences, ClassTypeDto ClassType)
        : base(CourseId, StudentGroupId, SectionId, PreferredTeacherId, Priority, StartDate, EndDate,
            DurationInPeriods, FrequencyPerWeek, RequiredCapacity, SoftPreferences, ClassType)
    {
        this.TimetableId = TimetableId;
    }
}

public record UpdateCourseRequirementRequestDto : CourseRequirementBaseDto
{
    public UpdateCourseRequirementRequestDto(
        int CourseId, int? StudentGroupId, int? SectionId, int? PreferredTeacherId,
        CourseRequirementPriority Priority, DateOnly StartDate, DateOnly EndDate, int DurationInPeriods,
        int FrequencyPerWeek, int? RequiredCapacity, List<SoftSchedulingPreferenceDto>? SoftPreferences, ClassTypeDto ClassType)
        : base(CourseId, StudentGroupId, SectionId, PreferredTeacherId, Priority, StartDate, EndDate,
            DurationInPeriods, FrequencyPerWeek, RequiredCapacity, SoftPreferences, ClassType)
    {
    }
}

// --- Sub-DTOs ---

public record SoftSchedulingPreferenceDto(
    [Required] string PreferenceType,
    [Required] string Value,
    int? Weight
);

// --- Response DTOs ---

public record CourseRequirementDto(
    int Id,
    int TimetableId,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CourseId,
    string? CourseName,
    int? StudentGroupId,
    string? StudentGroupName,
    int? SectionId,
    int? PreferredTeacherId,
    CourseRequirementPriority Priority,
    DateOnly StartDate,
    DateOnly EndDate,
    int DurationInPeriods,
    int FrequencyPerWeek,
    int? RequiredCapacity,
    ClassTypeDto ClassType,
    List<SoftSchedulingPreferenceDto>? SoftPreferences,
    EligibilitySummaryDto? EligibilitySummary
);

public record EligibilitySummaryDto(
    int Eligible,
    int Total,
    int Issues
);

public record PreflightCheckResultDto(
    EligibilitySummaryDto Summary,
    List<int> IneligibleStudentIds
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
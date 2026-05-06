using Domain.Enums;

public record TimeSlotDto(int DayId, int PeriodId);

public record TimetableCreateDto(
    string Name,
    string Timezone,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> Days,
    List<PeriodDto> Periods,
    List<string> SubjectIds);

public record PeriodDto(string Start, string End);

public record TeacherAvailabilityDto(string TeacherId, int DayId, int PeriodId, ConstraintLevel Level);

public record TeacherAssignmentDto(string TeacherId);

public record StudentAvailabilityDto(string StudentId, int DayId, int PeriodId, ConstraintLevel Level);

public record DeferredStudentDto(string StudentId, DeferralReason Reason);

public record StudentEnrollmentDto(string StudentId, string SubjectId);

public record CourseRequirementCreateDto(
    string SubjectId,
    string? TeacherId,
    string? StudentGroupId,
    int DurationInPeriods,
    int FrequencyPerWeek,
    string Priority,
    string ClassType);

// --- Read Models ---
public record TimetableSummaryDto(
    int Id,
    string Name,
    string Timezone,
    DateOnly StartDate,
    DateOnly EndDate);

public record TimetableDto(
    int Id,
    string Name,
    string Timezone,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> Days,
    List<PeriodDto> Periods,
    List<string> SubjectIds);

public record ActivityDto(
    int Id,
    string SubjectId,
    string? TeacherId,
    string? StudentGroupId,
    int DurationInPeriods,
    int FrequencyPerWeek,
    string Priority,
    string ClassType);

public class ActivitySummaryDto
{
    public string SubjectId { get; set; } = string.Empty;
    public string? TeacherId { get; set; }
    public string? StudentGroupId { get; set; }
    public string ClassType { get; set; } = string.Empty;
    public int FrequencyPerWeek { get; set; }
}
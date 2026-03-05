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
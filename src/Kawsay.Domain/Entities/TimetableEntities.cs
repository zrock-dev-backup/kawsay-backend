namespace Domain.Entities;

// STAGE 0: Configuration
public class TimetableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<TimetableDayEntity> Days { get; set; } = new();
    public List<TimetablePeriodEntity> Periods { get; set; } = new();
    public List<SubjectSelectionEntity> SelectedSubjects { get; set; } = new();
}

public class TimetableDayEntity
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimetableEntity Timetable { get; set; } = default!;
}

public class TimetablePeriodEntity
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public TimetableEntity Timetable { get; set; } = default!;
}

public class SubjectSelectionEntity
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string SubjectId { get; set; } = string.Empty; // External ID
    public TimetableEntity Timetable { get; set; } = default!;
}

// STAGE 1: Relational Mapping
public class TeacherAssignmentEntity // The 'Q' Set
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string TeacherId { get; set; } = string.Empty; // External ID
    public TimetableEntity Timetable { get; set; } = default!;
}

public class TeacherAvailabilityEntity // Base constraints for A_T
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public int DayId { get; set; }
    public int PeriodId { get; set; }
    public ConstraintLevel Level { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
}

public class StudentEnrollmentEntity // The 'E' Set
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public TimetableEntity Timetable { get; set; } = default!;
}

public class StudentAvailabilityEntity // Base constraints for A_S
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int DayId { get; set; }
    public int PeriodId { get; set; }
    public ConstraintLevel Level { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
}

public class DeferredStudentEntity // The 'D' Set
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Enums.DeferralReason Reason { get; set; }
    public TimetableEntity Timetable { get; set; } = default!;
}

// STAGE 2: Activity Modelling
public class CourseRequirementEntity // The 'Activity'
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public string SubjectId { get; set; } = string.Empty;
    public string? TeacherId { get; set; } // Nullable if not yet assigned
    public string? StudentGroupId { get; set; } // External Cohort/Group ID
    public int DurationInPeriods { get; set; }
    public int FrequencyPerWeek { get; set; }
    public string Priority { get; set; } = "Medium";
    public string ClassType { get; set; } = "Masterclass";
    public TimetableEntity Timetable { get; set; } = default!;
}

// STAGE 3: Generation State
public class StagedPlacementEntity
{
    public int Id { get; set; }
    public int CourseRequirementId { get; set; }
    public int DayId { get; set; }
    public int StartPeriodId { get; set; }
    public int Length { get; set; }
}
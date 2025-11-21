namespace Application.DTOs.V2;

// DTOs for Teacher Assignments, nested under Timetable
public record TimetableAssignmentDto(
    int AssignmentId,
    int TimetableId,
    int TeacherId,
    string TeacherFullName,
    int StartWeek,
    int EndWeek,
    int MaximumWorkload,
    string WorkloadUnit // "Classes" or "Hours per Week"
);

public record CreateTimetableAssignmentRequest(
    int TeacherId,
    int StartWeek,
    int EndWeek,
    int MaximumWorkload,
    string WorkloadUnit
);

// Other academic structure DTOs would follow this pattern...

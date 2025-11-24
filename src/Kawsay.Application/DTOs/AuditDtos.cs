namespace Application.DTOs;

public record StudentAuditDto(
    int StudentId,
    string StudentName,
    string StudentGroupName,
    string Status, // "ReadyToEnroll", "ActionRequired", "Enrolled"
    List<string>? Issues,
    DateTime LastUpdated
);

public record BulkEnrollmentRequest(
    int TimetableId,
    List<int> StudentIds
);

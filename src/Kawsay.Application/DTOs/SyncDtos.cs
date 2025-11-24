namespace Application.DTOs;

public record AcademicStructureSyncResultDto(
    string Source,
    DateTime StartedAt,
    DateTime CompletedAt,
    int ProcessedStudents,
    int CohortsCreated,
    int GroupsCreated,
    int SectionsCreated,
    string Message,
    List<string>? Warnings
);

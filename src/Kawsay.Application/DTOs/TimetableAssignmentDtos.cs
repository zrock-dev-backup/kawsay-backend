using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record TimetableAssignmentDto(
    int AssignmentId,
    int TimetableId,
    int TeacherId,
    int StartWeek,
    int EndWeek,
    int MaximumWorkload,
    string WorkloadUnit 
);

public record CreateTimetableAssignmentRequestDto(
    [Required] int TeacherId,
    [Required] [Range(1, 52)] int StartWeek,
    [Required] [Range(1, 52)] int EndWeek,
    [Required] [Range(1, 100)] int MaximumWorkload,
    [Required] string WorkloadUnit
);

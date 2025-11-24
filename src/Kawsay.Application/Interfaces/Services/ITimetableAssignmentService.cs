using Application.Core;
using Application.DTOs;

namespace Application.Interfaces.Services;

public interface ITimetableAssignmentService
{
    Task<Result<IEnumerable<TimetableAssignmentDto>>> GetAssignmentsAsync(int timetableId);
    Task<Result<TimetableAssignmentDto>> CreateAssignmentAsync(int timetableId, CreateTimetableAssignmentRequestDto request);
    Task<Result> DeleteAssignmentAsync(int assignmentId);
}

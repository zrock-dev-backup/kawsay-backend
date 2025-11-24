using Application.Core;
using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IStudentAuditService
{
    Task<Result<List<StudentAuditDto>>> GetStudentAuditAsync(int timetableId);
    Task<Result> BulkEnrollAsync(BulkEnrollmentRequest request);
}

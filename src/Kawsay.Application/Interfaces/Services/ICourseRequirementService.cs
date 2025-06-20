using Application.Core; // For Result
using Application.DTOs;

namespace Application.Interfaces.Services;

public interface ICourseRequirementService
{
    Task<Result<CourseRequirementDto>> CreateCourseRequirementAsync(int timetableId,
        CreateCourseRequirementRequestDto request);

    Task<Result<CourseRequirementDto>> GetCourseRequirementByIdAsync(int requirementId);
    Task<Result<IEnumerable<CourseRequirementDto>>> GetCourseRequirementsForTimetableAsync(int timetableId);

    Task<Result<CourseRequirementDto>> UpdateCourseRequirementAsync(int requirementId,
        UpdateCourseRequirementRequestDto request);

    Task<Result> DeleteCourseRequirementAsync(int requirementId); // Non-generic Result

    Task<Result<AvailableSlotsResponseDto>> GetAvailableSlotsForRequirementAsync(int requirementId,
        AvailableSlotsRequestDto slotRequest);
}
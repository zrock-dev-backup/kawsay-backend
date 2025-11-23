using Application.Core;
using Application.DTOs;

namespace Application.Interfaces.Services;

public interface ICourseRequirementService
{
    Task<Result<CourseRequirementDto>> CreateCourseRequirementAsync(CreateCourseRequirementRequestDto request);

    Task<Result<CourseRequirementDto>> GetCourseRequirementByIdAsync(int requirementId);

    Task<Result<IEnumerable<CourseRequirementDto>>> GetCourseRequirementsForTimetableAsync(int timetableId);

    Task<Result<CourseRequirementDto>> UpdateCourseRequirementAsync(int requirementId, UpdateCourseRequirementRequestDto request);

    Task<Result> DeleteCourseRequirementAsync(int requirementId);

    Task<Result<PreflightCheckResultDto>> RunPreflightCheckAsync(CreateCourseRequirementRequestDto request);
}

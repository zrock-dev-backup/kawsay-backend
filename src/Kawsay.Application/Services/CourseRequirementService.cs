using Application.Core;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Kawsay.Domain.ValueObjects;

namespace Application.Services;

public class CourseRequirementService(
    ICourseRequirementRepository requirementRepository,
    ITimetableRepository timetableRepository,
    ICourseRepository courseRepository,
    ISchedulingEngineService schedulingEngineService)
    : ICourseRequirementService
{

    public async Task<Result<CourseRequirementDto>> CreateCourseRequirementAsync(int timetableId,
        CreateCourseRequirementRequestDto request)
    {
        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null)
        {
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Timetable.NotFound",
                $"Timetable with ID {timetableId} not found."));
        }

        var course = await courseRepository.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Course.NotFound",
                $"Course with ID {request.CourseId} not found."));
        }

        try
        {
            _ = new DateRange(request.StartDate, request.EndDate);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRequirementDto>.Failure(Error.Validation("DateRange.Invalid", ex.Message));
        }

        if (request.StartDate < timetable.StartDate || request.EndDate > timetable.EndDate)
        {
            return Result<CourseRequirementDto>.Failure(Error.Validation("DateRange.OutOfBounds",
                "Requirement dates must be within the timetable's date range."));
        }
        var requirementEntity = CourseRequirementMapper.ToEntity(request, timetableId);
        await requirementRepository.AddAsync(requirementEntity);
        await requirementRepository.SaveChangesAsync();
        return Result<CourseRequirementDto>.Success(CourseRequirementMapper.ToDto(requirementEntity));
    }

    public async Task<Result<CourseRequirementDto>> GetCourseRequirementByIdAsync(int requirementId)
    {
        var requirementEntity = await requirementRepository.GetByIdAsync(requirementId);
        if (requirementEntity == null)
        {
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Requirement.NotFound",
                $"Course Requirement with ID {requirementId} not found."));
        }

        return Result<CourseRequirementDto>.Success(CourseRequirementMapper.ToDto(requirementEntity));
    }

    public async Task<Result<IEnumerable<CourseRequirementDto>>> GetCourseRequirementsForTimetableAsync(int timetableId)
    {
        var timetableExists = await timetableRepository.GetByIdAsync(timetableId); // Or a lighter ExistsAsync
        if (timetableExists == null)
        {
            return Result<IEnumerable<CourseRequirementDto>>.Failure(Error.NotFound("Timetable.NotFound",
                $"Timetable with ID {timetableId} not found."));
        }

        var requirementEntities = await requirementRepository.GetByTimetableIdAsync(timetableId);
        var dtos = requirementEntities.Select(CourseRequirementMapper.ToDto);
        return Result<IEnumerable<CourseRequirementDto>>.Success(dtos);
    }

    public async Task<Result<CourseRequirementDto>> UpdateCourseRequirementAsync(int requirementId,
        UpdateCourseRequirementRequestDto request)
    {
        var requirementEntity = await requirementRepository.GetByIdAsync(requirementId);
        if (requirementEntity == null)
        {
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Requirement.NotFound",
                $"Course Requirement with ID {requirementId} to update not found."));
        }

        if (requirementEntity.CourseId != request.CourseId)
        {
            var course = await courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                return Result<CourseRequirementDto>.Failure(Error.NotFound("Course.NotFound",
                    $"Course with ID {request.CourseId} not found for update."));
            }
        }

        // Validate DateRange
        try
        {
            _ = new DateRange(request.StartDate, request.EndDate);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRequirementDto>.Failure(Error.Validation("DateRange.Invalid", ex.Message));
        }
        // Potentially check against timetable dates if timetable context is available or fetched

        CourseRequirementMapper.UpdateEntityFromDto(requirementEntity, request);
        // _requirementRepository.UpdateAsync(requirementEntity); // EF Core tracks changes, so this might be redundant if entity is tracked
        await requirementRepository.SaveChangesAsync();

        return Result<CourseRequirementDto>.Success(CourseRequirementMapper.ToDto(requirementEntity));
    }

    public async Task<Result> DeleteCourseRequirementAsync(int requirementId)
    {
        var requirementEntity = await requirementRepository.GetByIdAsync(requirementId);
        if (requirementEntity == null)
        {
            return Result.Failure(Error.NotFound("Requirement.NotFound",
                $"Course Requirement with ID {requirementId} to delete not found."));
        }

        await requirementRepository.DeleteAsync(requirementEntity);
        await requirementRepository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<AvailableSlotsResponseDto>> GetAvailableSlotsForRequirementAsync(int requirementId,
        AvailableSlotsRequestDto slotRequest)
    {
        var requirementEntity = await requirementRepository.GetByIdAsync(requirementId);
        if (requirementEntity == null)
        {
            return Result<AvailableSlotsResponseDto>.Failure(Error.NotFound("Requirement.NotFound",
                $"Course Requirement with ID {requirementId} not found."));
        }

        return await schedulingEngineService.GetAvailableSlotsAsync(requirementEntity, slotRequest);
    }
}
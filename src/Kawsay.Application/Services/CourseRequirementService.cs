using Application.Core;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Kawsay.Domain.ValueObjects;

namespace Application.Services;

public class CourseRequirementService(
    ICourseRequirementRepository requirementRepository,
    ITimetableRepository timetableRepository,
    ICourseRepository courseRepository,
    // ISchedulingEngineService schedulingEngineService, // Temporarily commented out until Engine refactor
    IAcademicStructureRepository academicRepo
    )
    : ICourseRequirementService
{
    public async Task<Result<CourseRequirementDto>> CreateCourseRequirementAsync(CreateCourseRequirementRequestDto request)
    {
        var timetable = await timetableRepository.GetByIdAsync(request.TimetableId);
        if (timetable == null)
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Timetable.NotFound", $"Timetable {request.TimetableId} not found."));

        // Verify Dates
        if (request.StartDate < timetable.StartDate || request.EndDate > timetable.EndDate)
            return Result<CourseRequirementDto>.Failure(Error.Validation("DateRange.OutOfBounds", "Dates must be within the timetable range."));

        // Entity Mapping
        var entity = new CourseRequirementEntity
        {
            TimetableId = request.TimetableId,
            CourseId = request.CourseId,
            Course = courseRepository.GetByIdAsync(request.CourseId).Result!,
            StudentGroupId = request.StudentGroupId,
            SectionId = request.SectionId,
            PreferredTeacherId = request.PreferredTeacherId,
            Priority = request.Priority,
            EffectiveDateRange = new DateRange(request.StartDate, request.EndDate),
            DurationInPeriods = request.DurationInPeriods,
            FrequencyPerWeek = request.FrequencyPerWeek,
            RequiredCapacity = request.RequiredCapacity,
            Status = Kawsay.Domain.Enums.CourseRequirementStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add Soft Preferences
        if (request.SoftPreferences != null)
        {
            foreach (var pref in request.SoftPreferences)
            {
                entity.SoftPreferences.Add(new SoftSchedulingPreferenceEntity
                {
                    PreferenceType = pref.PreferenceType,
                    Value = pref.Value,
                    Weight = pref.Weight
                });
            }
        }

        await requirementRepository.AddAsync(entity);
        await requirementRepository.SaveChangesAsync();

        return Result<CourseRequirementDto>.Success(await MapToDto(entity));
    }

    public async Task<Result<CourseRequirementDto>> GetCourseRequirementByIdAsync(int requirementId)
    {
        var entity = await requirementRepository.GetByIdAsync(requirementId);
        if (entity == null)
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Requirement.NotFound", "Requirement not found."));

        return Result<CourseRequirementDto>.Success(await MapToDto(entity));
    }

    public async Task<Result<IEnumerable<CourseRequirementDto>>> GetCourseRequirementsForTimetableAsync(int timetableId)
    {
        var entities = await requirementRepository.GetByTimetableIdAsync(timetableId);
        var dtos = new List<CourseRequirementDto>();
        foreach (var e in entities) dtos.Add(await MapToDto(e));

        return Result<IEnumerable<CourseRequirementDto>>.Success(dtos);
    }

    public async Task<Result<CourseRequirementDto>> UpdateCourseRequirementAsync(int requirementId, UpdateCourseRequirementRequestDto request)
    {
        var entity = await requirementRepository.GetByIdAsync(requirementId);
        if (entity == null)
            return Result<CourseRequirementDto>.Failure(Error.NotFound("Requirement.NotFound", "Requirement not found."));

        entity.CourseId = request.CourseId;
        entity.StudentGroupId = request.StudentGroupId;
        entity.SectionId = request.SectionId;
        entity.PreferredTeacherId = request.PreferredTeacherId;
        entity.Priority = request.Priority;
        entity.EffectiveDateRange = new DateRange(request.StartDate, request.EndDate);
        entity.DurationInPeriods = request.DurationInPeriods;
        entity.FrequencyPerWeek = request.FrequencyPerWeek;
        entity.RequiredCapacity = request.RequiredCapacity;
        entity.UpdatedAt = DateTime.UtcNow;

        entity.SoftPreferences.Clear();
        if (request.SoftPreferences != null)
        {
            foreach (var pref in request.SoftPreferences)
            {
                entity.SoftPreferences.Add(new SoftSchedulingPreferenceEntity
                {
                    CourseRequirementId = entity.Id,
                    PreferenceType = pref.PreferenceType,
                    Value = pref.Value,
                    Weight = pref.Weight
                });
            }
        }

        await requirementRepository.SaveChangesAsync();
        return Result<CourseRequirementDto>.Success(await MapToDto(entity));
    }

    public async Task<Result> DeleteCourseRequirementAsync(int requirementId)
    {
        var entity = await requirementRepository.GetByIdAsync(requirementId);
        if (entity == null) return Result.Failure(Error.NotFound("NotFound", "Requirement not found"));

        await requirementRepository.DeleteAsync(entity);
        await requirementRepository.SaveChangesAsync();
        return Result.Success();
    }

    public Task<Result<PreflightCheckResultDto>> RunPreflightCheckAsync(CreateCourseRequirementRequestDto request)
    {
        // TODO: check if the student is eligible for the course
        // STUB: In a real scenario, check CoursePrerequisites vs StudentModuleGrades.
        // For now, return "All Eligible" to unblock the UI.
        var result = new PreflightCheckResultDto(
            new EligibilitySummaryDto(50, 50, 0),
            new List<int>()
        );
        return Task.FromResult(Result<PreflightCheckResultDto>.Success(result));
    }

    // Helper to enrich DTOs (could be moved to a Mapper class with Repo injection)
    private async Task<CourseRequirementDto> MapToDto(CourseRequirementEntity entity)
    {
        var course = await courseRepository.GetByIdAsync(entity.CourseId);
        string? groupName = null;
        if (entity.StudentGroupId.HasValue)
        {
            var group = await academicRepo.GetStudentGroupByIdAsync(entity.StudentGroupId.Value);
            groupName = group?.Name;
        }

        return new CourseRequirementDto(
            entity.Id,
            entity.TimetableId,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CourseId,
            course?.Name,
            entity.StudentGroupId,
            groupName,
            entity.SectionId,
            entity.PreferredTeacherId,
            entity.Priority,
            entity.EffectiveDateRange.StartDate,
            entity.EffectiveDateRange.EndDate,
            entity.DurationInPeriods,
            entity.FrequencyPerWeek,
            entity.RequiredCapacity,
            ClassTypeDto.Masterclass, // Defaulting for now, should be in Entity
            entity.SoftPreferences.Select(sp => new SoftSchedulingPreferenceDto(sp.PreferenceType, sp.Value, sp.Weight)).ToList(),
            null // EligibilitySummary is transient
        );
    }
}

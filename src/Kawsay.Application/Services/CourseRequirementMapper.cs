using Application.DTOs;
using Domain.Entities;
using Kawsay.Domain.ValueObjects;

namespace Application.Services;

public static class CourseRequirementMapper
{
    public static CourseRequirementDto ToDto(CourseRequirementEntity entity)
    {
        return new CourseRequirementDto(
            entity.Id,
            entity.TimetableId,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CourseId,
            entity.StudentGroupId,
            entity.SectionId,
            entity.PreferredTeacherId,
            entity.Priority,
            entity.EffectiveDateRange.StartDate,
            entity.EffectiveDateRange.EndDate,
            entity.DurationInPeriods,
            entity.FrequencyPerWeek,
            entity.RequiredCapacity,
            entity.SoftPreferences.Select(sp => new SoftSchedulingPreferenceDto(sp.PreferenceType, sp.Value, sp.Weight))
                .ToList()
        );
    }

    public static CourseRequirementEntity ToEntity(CreateCourseRequirementRequestDto dto, int timetableId)
    {
        var entity = new CourseRequirementEntity
        {
            TimetableId = timetableId,
            CourseId = dto.CourseId,
            StudentGroupId = dto.StudentGroupId,
            SectionId = dto.SectionId,
            PreferredTeacherId = dto.PreferredTeacherId,
            Priority = dto.Priority,
            EffectiveDateRange = new DateRange(dto.StartDate, dto.EndDate),
            DurationInPeriods = dto.DurationInPeriods,
            FrequencyPerWeek = dto.FrequencyPerWeek,
            RequiredCapacity = dto.RequiredCapacity,
            Status = Kawsay.Domain.Enums.CourseRequirementStatus.Pending, // Default status
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.SoftPreferences == null) return entity;
        foreach (var prefDto in dto.SoftPreferences)
        {
            entity.SoftPreferences.Add(new SoftSchedulingPreferenceEntity
            {
                PreferenceType = prefDto.PreferenceType,
                Value = prefDto.Value,
                Weight = prefDto.Weight
            });
        }

        return entity;
    }

    public static void UpdateEntityFromDto(CourseRequirementEntity entity, UpdateCourseRequirementRequestDto dto)
    {
        entity.CourseId = dto.CourseId;
        entity.StudentGroupId = dto.StudentGroupId;
        entity.SectionId = dto.SectionId;
        entity.PreferredTeacherId = dto.PreferredTeacherId;
        entity.Priority = dto.Priority;
        entity.EffectiveDateRange = new DateRange(dto.StartDate, dto.EndDate);
        entity.DurationInPeriods = dto.DurationInPeriods;
        entity.FrequencyPerWeek = dto.FrequencyPerWeek;
        entity.RequiredCapacity = dto.RequiredCapacity;
        
        entity.SoftPreferences.Clear();
        if (dto.SoftPreferences != null)
        {
            foreach (var prefDto in dto.SoftPreferences)
            {
                entity.SoftPreferences.Add(new SoftSchedulingPreferenceEntity
                {
                    CourseRequirementId = entity.Id,
                    PreferenceType = prefDto.PreferenceType,
                    Value = prefDto.Value,
                    Weight = prefDto.Weight
                });
            }
        }

        entity.UpdatedAt = DateTime.UtcNow;
    }
}
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ClassService(IClassRepository repository)
{
    public async Task<Class?> GetByIdAsync(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        return entity == null
            ? null
            : new Class
            {
                Id = entity.Id,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TimetableId = entity.TimetableId,
                CourseDto = new CourseDto
                {
                    Id = entity.Course.Id,
                    Name = entity.Course.Name,
                    Code = entity.Course.Code,
                },
                TeacherDto = entity.Teacher != null
                    ? new TeacherDto
                    {
                        Id = entity.Teacher.Id,
                    }
                    : null,
                ClassType = MapHelp(entity.ClassType),
                Length = entity.Length,
                Capacity = entity.Capacity,
                Frequency = entity.Frequency,
                ClassOccurrences = entity.ClassOccurrences.Select(occurence => new ClassOccurrenceDto
                    {
                        Date = occurence.Date,
                        StartPeriodId = occurence.StartPeriodId,
                    })
                    .ToList(),
                PeriodPreferences = entity.PeriodPreferences
            };
    }

    public async Task<IEnumerable<Class>> GetAllAsync(int timetableId)
    {
        var entities = await repository.GetAllAsync(timetableId);
        return entities.Select(entity => new Class
        {
            Id = entity.Id,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            TimetableId = entity.TimetableId,
            CourseDto = new CourseDto
            {
                Id = entity.Course.Id,
                Name = entity.Course.Name,
                Code = entity.Course.Code,
            },
            TeacherDto = entity.Teacher != null
                ? new TeacherDto
                {
                    Id = entity.Teacher.Id,
                }
                : null,
            ClassType = MapHelp(entity.ClassType),
            Length = entity.Length,
            Capacity = entity.Capacity,
            Frequency = entity.Frequency,
            ClassOccurrences = entity.ClassOccurrences.Select(occurence => new ClassOccurrenceDto
                {
                    Date = occurence.Date,
                    StartPeriodId = occurence.StartPeriodId,
                })
                .ToList(),
            PeriodPreferences = entity.PeriodPreferences
        });
    }

    public async Task<Class> CreateClassAsync(CreateClassRequest createRequest)
    {
        var entity = new ClassEntity
        {
            TimetableId = createRequest.TimetableId,
            CourseId = createRequest.CourseId,
            TeacherId = createRequest.TeacherId,
            StartDate = createRequest.StartDate,
            EndDate = createRequest.EndDate,
            Frequency = createRequest.Frequency,
            Length = createRequest.Length,
            Capacity = createRequest.Capacity,
            ClassType = MapHelp(createRequest.ClassType),
            PeriodPreferences = createRequest.PeriodPreferences.Select(p => new PeriodPreferenceEntity
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId,
            }).ToList()
        };

        var createdEntity = await repository.AddAsync(entity);

        return new Class
        {
            Id = createdEntity.Id,
            StartDate = createdEntity.StartDate,
            EndDate = createdEntity.EndDate,
            TimetableId = createdEntity.TimetableId,
            CourseDto = new CourseDto
            {
                Id = createdEntity.Course.Id,
                Name = createdEntity.Course.Name,
                Code = createdEntity.Course.Code,
            },
            TeacherDto = createdEntity.Teacher != null
                ? new TeacherDto
                {
                    Id = createdEntity.Teacher.Id,
                }
                : null,
            ClassType = MapHelp(createdEntity.ClassType),
            Length = createdEntity.Length,
            Frequency = createdEntity.Frequency,
            ClassOccurrences = createdEntity.ClassOccurrences.Select(o => new ClassOccurrenceDto
            {
                Date = o.Date,
                StartPeriodId = o.StartPeriodId,
            }).ToList(),
            PeriodPreferences = createdEntity.PeriodPreferences,
        };
    }
    
    public async Task<Class?> UpdateClassAsync(int classId, CreateClassRequest updateRequest)
    {
        var existingEntity = await repository.GetByIdAsync(classId);
        if (existingEntity == null)
        {
            return null;
        }

        existingEntity.CourseId = updateRequest.CourseId;
        existingEntity.TeacherId = updateRequest.TeacherId;
        existingEntity.Length = updateRequest.Length;
        existingEntity.Frequency = updateRequest.Frequency;
        existingEntity.ClassType = MapHelp(updateRequest.ClassType);
        existingEntity.StartDate = updateRequest.StartDate;
        existingEntity.EndDate = updateRequest.EndDate;

        existingEntity.PeriodPreferences.Clear();
        foreach (var p in updateRequest.PeriodPreferences)
        {
            existingEntity.PeriodPreferences.Add(new PeriodPreferenceEntity
            {
                DayId = p.DayId,
                StartPeriodId = p.StartPeriodId,
            });
        }
        
        await repository.UpdateAsync(existingEntity);
        return await GetByIdAsync(classId);
    }
    
    public async Task<bool> DeleteClassAsync(int classId)
    {
        var entity = await repository.GetByIdAsync(classId);
        if (entity == null)
        {
            return false;
        }
        await repository.DeleteAsync(entity);
        return true;
    }

    private static ClassType MapHelp(ClassTypeDto dto)
    {
        return dto switch
        {
            ClassTypeDto.Lab => ClassType.Lab,
            ClassTypeDto.Masterclass => ClassType.Masterclass,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), $"Not a valid ClassTypeDto value: {dto}")
        };
    }

    private static ClassTypeDto MapHelp(ClassType dto)
    {
        return dto switch
        {
            ClassType.Lab => ClassTypeDto.Lab,
            ClassType.Masterclass => ClassTypeDto.Masterclass,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), $"Not a valid ClassType value: {dto}")
        };
    }
}
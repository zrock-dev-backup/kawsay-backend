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
                TimetableId = entity.TimetableId,
                CourseDto = new CourseDto
                {
                    Id = entity.Course.Id,
                    Name = entity.Course.Name,
                    Code = entity.Course.Code,
                },
                TeacherDto = new TeacherDto
                {
                    Id = entity.Teacher.Id,
                    Name = entity.Teacher.Name,
                    Type = entity.Teacher.Type,
                },
                Length = entity.Length,
                Frequency = entity.Frequency,
                ClassOccurrences = entity.ClassOccurrences.Select(occurence => new ClassOccurrenceDto
                {
                    Date = occurence.Date,
                    StartPeriodId = occurence.StartPeriodId,
                }).ToList(),
                PeriodPreferences = entity.PeriodPreferences
            };
    }

    public async Task<IEnumerable<Class>> GetAllAsync(int timetableId)
    {
        var entities = await repository.GetAllAsync(timetableId);
        return entities.Select(entity => new Class
        {
            Id = entity.Id,
            TimetableId = entity.TimetableId,
            CourseDto = new CourseDto
            {
                Id = entity.Course.Id,
                Name = entity.Course.Name,
                Code = entity.Course.Code,
            },
            TeacherDto = new TeacherDto
            {
                Id = entity.Teacher.Id,
                Name = entity.Teacher.Name,
                Type = entity.Teacher.Type,
            },
            Length = entity.Length,
            Frequency = entity.Frequency,
            ClassOccurrences = entity.ClassOccurrences.Select(occurence => new ClassOccurrenceDto
            {
                Date = occurence.Date,
                StartPeriodId = occurence.StartPeriodId,
            }).ToList(),
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
            Frequency = createRequest.Frequency,
            Length = createRequest.Length,
            ClassType = MapHelp(createRequest.ClassType),
            StudentGroupId = createRequest.StudentGroupId,
            SectionId = createRequest.SectionId,
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
            TimetableId = createdEntity.TimetableId,
            CourseDto = new CourseDto
            {
                Id = createdEntity.Course.Id,
                Name = createdEntity.Course.Name,
                Code = createdEntity.Course.Code,
            },
            TeacherDto = createdEntity.Teacher != null ? new TeacherDto
            {
                Id = createdEntity.Teacher.Id,
                Name = createdEntity.Teacher.Name,
                Type = createdEntity.Teacher.Type,
            } : null,
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

    private static ClassType MapHelp(ClassTypeDto dto)
    {
        return dto switch
        {
            ClassTypeDto.Lab => ClassType.Lab,
            ClassTypeDto.Masterclass => ClassType.Masterclass,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private static ClassTypeDto MapHelp(ClassType dto)
    {
        return dto switch
        {
            ClassType.Lab => ClassTypeDto.Lab,
            ClassType.Masterclass => ClassTypeDto.Masterclass,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class ConfigurationService(
    IClassTypeConfigurationRepository classTypeConfigRepo,
    ITeacherQualificationRepository teacherQualificationRepo)
{
    public async Task<IEnumerable<ClassTypeConfigurationDto>> GetAllClassTypeConfigurationsAsync()
    {
        var configs = await classTypeConfigRepo.GetAllAsync();
        return configs.Select(c => new ClassTypeConfigurationDto
        {
            ClassType = c.ClassType == Domain.Enums.ClassType.Masterclass ? ClassTypeDto.Masterclass : ClassTypeDto.Lab,
            DefaultLength = c.DefaultLength
        });
    }

    public async Task AddTeacherQualificationsAsync(CreateTeacherQualificationRequest request)
    {
        var newQualifications = request.CourseIds
            .Select(courseId => new TeacherQualificationEntity
            {
                TeacherId = request.TeacherId,
                CourseId = courseId
            }).ToList();

        if (newQualifications.Count == 0)
        {
            throw new ArgumentException("At least one CourseId must be provided.");
        }

        await teacherQualificationRepo.AddRangeAsync(newQualifications);
    }

    public async Task SetClassTypeConfigurationsAsync(IEnumerable<SetClassTypeConfigurationRequest> requests)
    {
        var configurationsToUpsert = requests.Select(request => new ClassTypeConfigurationEntity
            {
                ClassType = request.ClassType == ClassTypeDto.Masterclass
                    ? Domain.Enums.ClassType.Masterclass
                    : Domain.Enums.ClassType.Lab,
                DefaultLength = request.DefaultLength
            })
            .ToList();

        await classTypeConfigRepo.UpsertRangeAsync(configurationsToUpsert);
    }
}
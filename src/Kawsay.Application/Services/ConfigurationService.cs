using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class ConfigurationService(
    IClassTypeConfigurationRepository classTypeConfigRepo
)
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
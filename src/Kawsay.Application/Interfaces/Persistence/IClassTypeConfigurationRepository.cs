using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IClassTypeConfigurationRepository
{
    Task<ClassTypeConfigurationEntity?> GetByTypeAsync(Domain.Enums.ClassType classType);
    Task<List<ClassTypeConfigurationEntity>> GetAllAsync();
    Task UpsertRangeAsync(IEnumerable<ClassTypeConfigurationEntity> configurations);
}

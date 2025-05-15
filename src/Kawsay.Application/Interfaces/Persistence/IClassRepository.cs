using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IClassRepository
{
    Task<ClassEntity?> GetByIdAsync(int id);
    Task<IEnumerable<ClassEntity>> GetAllAsync();
    Task<ClassEntity> AddAsync(ClassEntity lecture);
}
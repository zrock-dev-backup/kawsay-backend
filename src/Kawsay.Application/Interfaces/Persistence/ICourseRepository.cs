using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ICourseRepository
{
    Task<CourseEntity?> GetByIdAsync(int id);
    Task<IEnumerable<CourseEntity>> GetAllAsync();
    Task<CourseEntity> AddAsync(CourseEntity course);
}
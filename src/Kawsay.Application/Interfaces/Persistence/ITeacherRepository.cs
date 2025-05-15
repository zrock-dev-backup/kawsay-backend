using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ITeacherRepository
{
    Task<TeacherEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TeacherEntity>> GetAllAsync();
    Task<TeacherEntity> AddAsync(TeacherEntity teacher);
}
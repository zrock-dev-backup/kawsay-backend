using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IStudentRepository
{
    Task<StudentEntity?> GetByIdAsync(int id);
    Task<IEnumerable<StudentEntity>> GetAllAsync();
    Task<StudentEntity> AddAsync(StudentEntity student);
    Task UpdateAsync(StudentEntity student);
    Task UpdateRangeAsync(IEnumerable<StudentEntity> students);
}

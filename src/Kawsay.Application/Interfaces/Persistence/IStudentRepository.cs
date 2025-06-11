using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IStudentRepository
{
    Task<StudentEntity?> GetByIdAsync(int id);
    Task<List<StudentEntity>> GetByIdsAsync(IEnumerable<int> studentIds); 
    Task<IEnumerable<StudentEntity>> GetAllAsync();
    Task<StudentEntity> AddAsync(StudentEntity student);
    Task UpdateAsync(StudentEntity student);
    Task UpdateRangeAsync(IEnumerable<StudentEntity> students);
}

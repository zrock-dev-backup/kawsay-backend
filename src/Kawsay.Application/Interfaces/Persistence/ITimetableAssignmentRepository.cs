using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ITimetableAssignmentRepository
{
    Task<IEnumerable<TimetableAssignmentEntity>> GetByTimetableIdAsync(int timetableId);
    Task<TimetableAssignmentEntity?> GetByIdAsync(int id);
    Task<TimetableAssignmentEntity> AddAsync(TimetableAssignmentEntity entity);
    Task DeleteAsync(TimetableAssignmentEntity entity);
    Task SaveChangesAsync();
}

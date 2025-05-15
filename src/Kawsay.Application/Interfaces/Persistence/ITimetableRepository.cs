using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ITimetableRepository
{
    Task<TimetableEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TimetableEntity>> GetAllAsync();
    Task<TimetableEntity> AddAsync(TimetableEntity timetable);
}
using Domain.Entities;
namespace Application.Interfaces.Persistence;
public interface ICourseRequirementRepository
{
    Task<CourseRequirementEntity> AddAsync(CourseRequirementEntity requirement);
    Task<CourseRequirementEntity?> GetByIdAsync(int requirementId);
    Task<IEnumerable<CourseRequirementEntity>> GetByTimetableIdAsync(int timetableId);
    Task UpdateAsync(CourseRequirementEntity requirement); // Note: EF Core tracks changes, explicit Update might not always be needed if entity is tracked
    Task DeleteAsync(CourseRequirementEntity requirement);
    Task<bool> ExistsAsync(int requirementId);
    Task<int> SaveChangesAsync(); // For Unit of Work pattern
}

using Domain.Entities.availability;

public interface IFacultyAvailabilityRepository
{
    Task AddRangeAsync(IEnumerable<AvailabilityEntity> entities);
    Task<List<AvailabilityEntity>> GetByFacultyIdAsync(int facultyId);
    Task<List<AvailabilityEntity>> GetAllAsync();
    Task<AvailabilityEntity?> GetByIdAsync(int id);
    Task DeleteAsync(AvailabilityEntity entity);
}

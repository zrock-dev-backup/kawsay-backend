using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IEnrollmentRepository
{
    Task<EnrollmentEntity> AddAsync(EnrollmentEntity enrollment);
    Task<List<EnrollmentEntity>> GetEnrollmentsForStudentAsync(int studentId, int timetableId);
    Task<int> CountByClassIdAsync(int classId);
}

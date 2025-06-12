using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ITeacherQualificationRepository
{
    Task<List<TeacherEntity>> GetQualifiedTeachersForCourseAsync(int courseId);
    Task AddRangeAsync(IEnumerable<TeacherQualificationEntity> qualifications);
}

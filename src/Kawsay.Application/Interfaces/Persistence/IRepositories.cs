using Domain.Entities;
namespace Application.Interfaces.Persistence;
public interface ITimetableRepository
{
Task<TimetableEntity?> GetByIdAsync(int id);
Task<List<TimetableEntity>> GetAllAsync();
Task AddAsync(TimetableEntity entity);
}
public interface IRelationalMappingRepository
{
Task AddTeacherAssignmentAsync(TeacherAssignmentEntity entity);
Task<List<string>> GetTeacherAssignmentsAsync(int timetableId);
Task AddTeacherAvailabilityAsync(TeacherAvailabilityEntity entity);
Task<List<TeacherAvailabilityEntity>> GetTeacherAvailabilitiesAsync(int timetableId, string teacherId);

Task AddStudentEnrollmentAsync(StudentEnrollmentEntity entity);
Task<List<StudentEnrollmentEntity>> GetStudentEnrollmentsAsync(int timetableId, string studentId);

Task AddStudentAvailabilityAsync(StudentAvailabilityEntity entity);
Task<List<StudentAvailabilityEntity>> GetStudentAvailabilitiesAsync(int timetableId, string studentId);

Task AddDeferredStudentAsync(DeferredStudentEntity entity);
Task<List<DeferredStudentEntity>> GetDeferredStudentsAsync(int timetableId);
}
public interface ICourseRequirementRepository
{
Task AddAsync(CourseRequirementEntity entity);
Task<List<CourseRequirementEntity>> GetByTimetableIdAsync(int timetableId);
}
public interface IStagedPlacementRepository
{
Task AddAsync(StagedPlacementEntity entity);
Task DeleteAllForTimetableAsync(int timetableId);
}

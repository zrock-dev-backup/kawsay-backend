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

    // Teacher Availability
    Task AddTeacherAvailabilityAsync(TeacherAvailabilityEntity entity);

    Task<TeacherAvailabilityEntity?> GetTeacherAvailabilityAsync(int timetableId, string teacherId, int dayId,
        int periodId);

    Task<List<TeacherAvailabilityEntity>> GetTeacherAvailabilitiesAsync(int timetableId, string teacherId);
    void RemoveTeacherAvailability(TeacherAvailabilityEntity entity);

    // Student Enrollment
    Task AddStudentEnrollmentAsync(StudentEnrollmentEntity entity);
    Task<List<StudentEnrollmentEntity>> GetStudentEnrollmentsAsync(int timetableId, string studentId);

    // Student Availability
    Task AddStudentAvailabilityAsync(StudentAvailabilityEntity entity);

    Task<StudentAvailabilityEntity?> GetStudentAvailabilityAsync(int timetableId, string studentId, int dayId,
        int periodId);

    Task<List<StudentAvailabilityEntity>> GetStudentAvailabilitiesAsync(int timetableId, string studentId);
    void RemoveStudentAvailability(StudentAvailabilityEntity entity);

    // Deferred Students
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
    Task<List<StagedPlacementEntity>> GetByTimetableIdAsync(int timetableId);
}
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TimetableRepository(KawsayDbContext context) : ITimetableRepository
{
    public async Task<TimetableEntity?> GetByIdAsync(int id)
    {
        return await context.Timetables
            .Include(t => t.Days)
            .Include(t => t.Periods)
            .Include(t => t.SelectedSubjects)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TimetableEntity>> GetAllAsync()
    {
        return await context.Timetables.ToListAsync();
    }

    public async Task AddAsync(TimetableEntity entity)
    {
        await context.Timetables.AddAsync(entity);
    }
}

public class RelationalMappingRepository(KawsayDbContext context) : IRelationalMappingRepository
{
    public async Task AddTeacherAssignmentAsync(TeacherAssignmentEntity entity) =>
        await context.TeacherAssignments.AddAsync(entity);

    public async Task<List<string>> GetTeacherAssignmentsAsync(int timetableId) =>
        await context.TeacherAssignments.Where(t => t.TimetableId == timetableId).Select(t => t.TeacherId)
            .ToListAsync();

    public async Task AddTeacherAvailabilityAsync(TeacherAvailabilityEntity entity) =>
        await context.TeacherAvailabilities.AddAsync(entity);

    public async Task<List<TeacherAvailabilityEntity>>
        GetTeacherAvailabilitiesAsync(int timetableId, string teacherId) =>
        await context.TeacherAvailabilities.Where(t => t.TimetableId == timetableId && t.TeacherId == teacherId)
            .ToListAsync();

    public async Task AddStudentEnrollmentAsync(StudentEnrollmentEntity entity) =>
        await context.StudentEnrollments.AddAsync(entity);

    public async Task<List<StudentEnrollmentEntity>> GetStudentEnrollmentsAsync(int timetableId, string studentId) =>
        await context.StudentEnrollments.Where(e => e.TimetableId == timetableId && e.StudentId == studentId)
            .ToListAsync();

    public async Task AddStudentAvailabilityAsync(StudentAvailabilityEntity entity) =>
        await context.StudentAvailabilities.AddAsync(entity);

    public async Task<List<StudentAvailabilityEntity>>
        GetStudentAvailabilitiesAsync(int timetableId, string studentId) =>
        await context.StudentAvailabilities.Where(a => a.TimetableId == timetableId && a.StudentId == studentId)
            .ToListAsync();

    public async Task AddDeferredStudentAsync(DeferredStudentEntity entity) =>
        await context.DeferredStudents.AddAsync(entity);

    public async Task<List<DeferredStudentEntity>> GetDeferredStudentsAsync(int timetableId) =>
        await context.DeferredStudents.Where(d => d.TimetableId == timetableId).ToListAsync();

    public class CourseRequirementRepository(KawsayDbContext context) : ICourseRequirementRepository
    {
        public async Task AddAsync(CourseRequirementEntity entity) => await context.CourseRequirements.AddAsync(entity);

        public async Task<List<CourseRequirementEntity>> GetByTimetableIdAsync(int timetableId) =>
            await context.CourseRequirements.Where(c => c.TimetableId == timetableId).ToListAsync();
    }

    public class StagedPlacementRepository(KawsayDbContext context) : IStagedPlacementRepository
    {
        public async Task AddAsync(StagedPlacementEntity entity) => await context.StagedPlacements.AddAsync(entity);

        public async Task DeleteAllForTimetableAsync(int timetableId)
        {
            var placements = await context.StagedPlacements
                .Join(context.CourseRequirements, p => p.CourseRequirementId, c => c.Id, (p, c) => new { p, c })
                .Where(x => x.c.TimetableId == timetableId)
                .Select(x => x.p)
                .ToListAsync();

            context.StagedPlacements.RemoveRange(placements);
        }
    }
}
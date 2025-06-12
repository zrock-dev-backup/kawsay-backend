using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeacherQualificationRepository(KawsayDbContext context) : ITeacherQualificationRepository
{
    public async Task<List<TeacherEntity>> GetQualifiedTeachersForCourseAsync(int courseId)
    {
        return await context.TeacherQualifications
            .Where(tq => tq.CourseId == courseId)
            .Select(tq => tq.Teacher)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<TeacherQualificationEntity> qualifications)
    {
        await context.TeacherQualifications.AddRangeAsync(qualifications);
        await context.SaveChangesAsync();
    }
}

using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EnrollmentRepository(KawsayDbContext context) : IEnrollmentRepository
{
    public async Task<EnrollmentEntity> AddAsync(EnrollmentEntity enrollment)
    {
        await context.Enrollments.AddAsync(enrollment);
        await context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<List<EnrollmentEntity>> GetEnrollmentsForStudentAsync(int studentId, int timetableId)
    {
        return await context.Enrollments
            .Where(e => e.StudentId == studentId && e.Class.TimetableId == timetableId)
            .Include(e => e.Class)
            .ThenInclude(c => c.ClassOccurrences)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountByClassIdAsync(int classId)
    {
        return await context.Enrollments.CountAsync(e => e.ClassId == classId);
    }
}
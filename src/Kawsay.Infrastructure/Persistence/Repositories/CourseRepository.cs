using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseRepository(KawsayDbContext context) : ICourseRepository
{
    public async Task<CourseEntity?> GetByIdAsync(int id)
    {
        return await context.Courses.FindAsync(id);
    }

    public async Task<IEnumerable<CourseEntity>> GetAllAsync()
    {
        return await context.Courses.ToListAsync();
    }

    public async Task<CourseEntity> AddAsync(CourseEntity course)
    {
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        return course;
    }
}
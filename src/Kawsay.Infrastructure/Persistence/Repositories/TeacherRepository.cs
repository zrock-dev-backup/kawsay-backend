using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TeacherRepository(KawsayDbContext context) : ITeacherRepository
{
    public async Task<TeacherEntity?> GetByIdAsync(int id)
    {
        return await context.Teachers.FindAsync(id);
    }

    public async Task<IEnumerable<TeacherEntity>> GetAllAsync()
    {
        return await context.Teachers.ToListAsync();
    }

    public async Task<TeacherEntity> AddAsync(TeacherEntity teacher)
    {
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();
        return teacher;
    }
}
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentRepository(KawsayDbContext context) : IStudentRepository
{
    public async Task<StudentEntity?> GetByIdAsync(int id)
    {
        return await context.Students.FindAsync(id);
    }

    public async Task<IEnumerable<StudentEntity>> GetAllAsync()
    {
        return await context.Students.ToListAsync();
    }

    public async Task<StudentEntity> AddAsync(StudentEntity student)
    {
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();
        return student;
    }

    public async Task UpdateAsync(StudentEntity student)
    {
        context.Entry(student).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<StudentEntity> students)
    {
        context.Students.UpdateRange(students);
        await context.SaveChangesAsync();
    }
}

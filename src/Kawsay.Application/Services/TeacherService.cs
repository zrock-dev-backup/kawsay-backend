using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class TeacherService(
    ITeacherRepository repository
)
{
    
    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        return entity == null
            ? null
            : new TeacherDto
            {
                Id = entity.Id,
            };
    }

    public async Task<IEnumerable<TeacherDto>> GetAllAsync()
    {
        var entity = await repository.GetAllAsync();
        return entity.Select(teacher => new TeacherDto
        {
            Id = teacher.Id,
        });
    }
}

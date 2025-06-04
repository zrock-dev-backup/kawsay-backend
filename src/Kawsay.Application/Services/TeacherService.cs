using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class TeacherService(ITeacherRepository repository)
{
    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        return entity == null
            ? null
            : new TeacherDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
            };
    }

    public async Task<IEnumerable<TeacherDto>> GetAllAsync()
    {
        var entity = await repository.GetAllAsync();
        return entity.Select(teacher => new TeacherDto
        {
            Id = teacher.Id,
            Name = teacher.Name,
            Type = teacher.Type,
        });
    }

    public async Task<TeacherDto> CreateCourseAsync(TeacherDto teacher)
    {
        var courseEntity = new TeacherEntity
        {
            Name = teacher.Name,
            Type = teacher.Type
        };

        var createdEntity = await repository.AddAsync(courseEntity);
        return new TeacherDto
        {
            Id = createdEntity.Id,
            Name = createdEntity.Name,
            Type = createdEntity.Type
        };
    }
}
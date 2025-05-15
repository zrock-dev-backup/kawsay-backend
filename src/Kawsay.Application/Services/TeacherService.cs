using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class TeacherService(ITeacherRepository repository)
{
    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        var courseEntity = await repository.GetByIdAsync(id);
        return courseEntity == null
            ? null
            : new TeacherDto
            {
                Id = courseEntity.Id,
                Name = courseEntity.Name,
            };
    }

    public async Task<IEnumerable<TeacherDto>> GetAllAsync()
    {
        var courseEntities = await repository.GetAllAsync();
        return courseEntities.Select(e => new TeacherDto
        {
            Id = e.Id,
            Name = e.Name,
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
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class CourseService(ICourseRepository courseRepository)
{
    public async Task<CourseDto?> GetCourseByIdAsync(int id)
    {
        var courseEntity = await courseRepository.GetByIdAsync(id);
        return courseEntity == null
            ? null
            : new CourseDto
            {
                Id = courseEntity.Id,
                Name = courseEntity.Name,
                Code = courseEntity.Code
            };
    }

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
    {
        var courseEntities = await courseRepository.GetAllAsync();
        return courseEntities.Select(e => new CourseDto
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code
        });
    }

    public async Task<CourseDto> CreateCourseAsync(CourseDto createCourseRequestDto)
    {
        var courseEntity = new CourseEntity
        {
            Name = createCourseRequestDto.Name,
            Code = createCourseRequestDto.Code
        };

        var createdEntity = await courseRepository.AddAsync(courseEntity);
        return new CourseDto()
        {
            Id = createdEntity.Id,
            Name = createdEntity.Name,
            Code = createdEntity.Code
        };
    }
}
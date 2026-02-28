using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CourseService(
    ICourseRepository courseRepository,
    ITeacherQualificationRepository qualificationRepository)
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

    public async Task<IEnumerable<TeacherDto>> GetQualifiedTeachersForCourseAsync(int courseId)
    {
        var teachers = await qualificationRepository.GetQualifiedTeachersForCourseAsync(courseId);
        return teachers.Select(t => new TeacherDto
        {
            Id = t.Id,
            Name = t.Name,
            Type = t.Type
        });
    }
}
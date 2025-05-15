using Application.DTOs;
using Domain.Entities;

namespace Application.Models;

public class Class
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public CourseDto CourseDto { get; set; } = new();
    public TeacherDto TeacherDto { get; set; } = new();
    public int Length { get; set; }
    public int Frequency { get; set; }
    public ICollection<ClassOccurrenceDto> ClassOccurrences { get; set; } = new List<ClassOccurrenceDto>();
    public ICollection<PeriodPreferenceEntity> PeriodPreferences { get; set; } = new List<PeriodPreferenceEntity>();
}
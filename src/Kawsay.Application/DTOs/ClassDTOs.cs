namespace Application.DTOs;

public class DayPeriodPreferenceDto
{
    public int DayId { get; set; }
    public int StartPeriodId { get; set; }
}

public class ClassDto
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public CourseDto CourseDto { get; set; } = new();
    public TeacherDto TeacherDto { get; set; } = new();
    public int Length { get; set; }
    public int Frequency { get; set; }
    public ICollection<ClassOccurrenceDto> ClassOccurrences { get; set; } = new List<ClassOccurrenceDto>();
    public ICollection<DayPeriodPreferenceDto> PeriodPreferences { get; set; } = new List<DayPeriodPreferenceDto>();
}

public class CreateClassRequest
{
    public int TimetableId { get; set; }
    public int CourseId { get; set; }
    public int TeacherId { get; set; }
    public int Length { get; set; }
    public int Frequency { get; set; }
    public List<DayPeriodPreferenceDto> PeriodPreferences { get; set; } = [];
}
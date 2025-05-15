namespace Application.DTOs;

public class ClassDto
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public CourseDto CourseDto { get; set; } = new();
    public Teacher Teacher { get; set; }
    public int Length { get; set; }
    public int Frequency { get; set; }
    public ICollection<ClassOccurrenceDto> ClassOccurrences { get; set; } = new List<ClassOccurrenceDto>();
}

public class PeriodPreferencesDto
{
    public int StartPeriodId { get; set; }
}

public class CreateClassRequest
{
    public int TimetableId { get; set; }
    public int CourseId { get; set; }
    public int TeacherId { get; set; }
    public int Length { get; set; }
    public int Frequency { get; set; }
    public List<PeriodPreferencesDto> PeriodPreferencesList { get; set; } = [];
}
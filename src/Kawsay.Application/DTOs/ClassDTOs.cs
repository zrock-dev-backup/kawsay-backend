namespace Application.DTOs;

public class DayPeriodPreferenceDto
{
    public int DayId { get; set; }
    public int StartPeriodId { get; set; }
}

public enum ClassTypeDto
{
    Masterclass,
    Lab
}

public class ClassDto
{
    public int Id { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int TimetableId { get; set; }
    
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
        
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    
    public int Length { get; set; }
    public int Frequency { get; set; }
    public int Capacity { get; set; }
    public int? StudentGroupId { get; set; }
    public int? SectionId { get; set; }
    public ClassTypeDto ClassType { get; set; }
    public ICollection<ClassOccurrenceDto> ClassOccurrences { get; set; } = new List<ClassOccurrenceDto>();
    public ICollection<DayPeriodPreferenceDto> PeriodPreferences { get; set; } = new List<DayPeriodPreferenceDto>();
}

public class CreateClassRequest
{
    public int TimetableId { get; set; }
    public int CourseId { get; set; }
    public int? TeacherId { get; set; }
    public int Length { get; set; }
    public int Frequency { get; set; }
    public int Capacity { get; set; }
    public ClassTypeDto ClassType { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<DayPeriodPreferenceDto> PeriodPreferences { get; set; } = [];
}
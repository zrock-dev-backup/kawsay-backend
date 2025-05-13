using kawsay.Entities;

namespace kawsay.DTOs;

public class ClassDto
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public Course Course { get; set; } = new();
    public Teacher Teacher { get; set; }
    public int Length { get; set; }
    public int Frequency { get; set; }
    public List<PeriodPreferencesDto> PeriodPreferencesList { get; set; } = [];
    public ICollection<ClassOccurrence> ClassOccurrences { get; set; } = new List<ClassOccurrence>();
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

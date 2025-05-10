namespace kawsay.DTOs;

public class ClassOccurrence
{
    public int Id { get; set; }
    public int DayId { get; set; }
    public int StartPeriodId { get; set; }
    public int Length { get; set; }
}

public class Class
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public Course Course { get; set; } = new();
    public Teacher? Teacher { get; set; }
    public List<ClassOccurrence> Occurrences { get; set; } = new();
}

public class CreateClassOccurrenceDto
{
    public int DayId { get; set; }
    public int StartPeriodId { get; set; }
    public int Length { get; set; }
}

public class CreateClassRequest
{
    public int TimetableId { get; set; }
    public int CourseId { get; set; }
    public int? TeacherId { get; set; }
    public List<CreateClassOccurrenceDto> Occurrences { get; set; } = new();
}

public class UpdateClassRequest
{
    public int Id { get; set; }
    public int TimetableId { get; set; }
    public int CourseId { get; set; }
    public int? TeacherId { get; set; }
    public List<ClassOccurrence> Occurrences { get; set; } = new();
}
namespace Domain.Entities;

public class TeacherQualificationEntity
{
    public int TeacherId { get; set; }
    public TeacherEntity Teacher { get; set; } = default!;

    public int CourseId { get; set; }
    public CourseEntity Course { get; set; } = default!;
}

namespace Application.DTOs;

public class CreateTeacherQualificationRequest
{
    public int TeacherId { get; set; }
    public List<int> CourseIds { get; set; } = [];
}

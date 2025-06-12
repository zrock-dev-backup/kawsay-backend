namespace Application.DTOs;

public class CreateCourseConfigurationRequest
{
    public int CourseId { get; set; }
    public ClassTypeDto ClassType { get; set; }
    public int DefaultLength { get; set; }
    public int DefaultFrequency { get; set; }
}

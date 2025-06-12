using Application.DTOs;

namespace Application.DTOs;

public class CourseConfigurationDto
{
    public int Id { get; set; }
    public ClassTypeDto ClassType { get; set; }
    public int DefaultLength { get; set; }
    public int DefaultFrequency { get; set; }
}

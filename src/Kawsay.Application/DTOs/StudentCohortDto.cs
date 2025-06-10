namespace Application.DTOs;

public class StudentCohortDto
{
    public List<StudentDto> AdvancingStudents { get; set; } = [];
    public List<StudentDto> RetakeStudents { get; set; } = [];
}

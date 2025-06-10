namespace Application.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Standing { get; set; } = string.Empty; // Using string to represent the enum
}

namespace Application.DTOs;

public class GradeIngestionDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal GradeValue { get; set; }
}

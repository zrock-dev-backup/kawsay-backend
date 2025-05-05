// DTOs/BasicEntities.cs
namespace KawsayApiMockup.DTOs
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Professor" or "Faculty Practitioner"
    }
}

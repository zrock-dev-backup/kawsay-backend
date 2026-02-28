using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Models.External;

public class CurriculumCatalog
{
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("terms")] public List<Term> Terms { get; set; } = new();
    [JsonPropertyName("english_for_specific_purposes")] public ESP? Esp { get; set; }
}

public class Term
{
    [JsonPropertyName("term")] public int TermNumber { get; set; }
    [JsonPropertyName("courses")] public List<Course> Courses { get; set; } = new();
}

public class ESP { [JsonPropertyName("levels")] public List<ESPLevel> Levels { get; set; } = new(); }
public class ESPLevel { [JsonPropertyName("level")] public int Level { get; set; } [JsonPropertyName("courses")] public List<Course> Courses { get; set; } = new(); }

public class Course
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    
    // Handles Mixed Type support safely (int vs string)
    [JsonPropertyName("credit_hours")] public JsonElement CreditHours { get; set; }
}

public class Cohort
{
    [JsonPropertyName("cohort_id")] public string CohortId { get; set; } = string.Empty;
    [JsonPropertyName("cohort_name")] public string CohortName { get; set; } = string.Empty;
    [JsonPropertyName("academic_year")] public string AcademicYear { get; set; } = string.Empty;
    [JsonPropertyName("groups")] public List<Group> Groups { get; set; } = new();
}

public class Group
{
    [JsonPropertyName("group_id")] public string GroupId { get; set; } = string.Empty;
    [JsonPropertyName("group_name")] public string GroupName { get; set; } = string.Empty;
    [JsonPropertyName("labs")] public List<Lab> Labs { get; set; } = new();
}

public class Lab
{
    [JsonPropertyName("lab_id")] public string LabId { get; set; } = string.Empty;
    [JsonPropertyName("lab_name")] public string LabName { get; set; } = string.Empty;
    [JsonPropertyName("capacity")] public int Capacity { get; set; }
    [JsonPropertyName("students")] public List<Student> Students { get; set; } = new();
}

public class Student
{
    [JsonPropertyName("student_id")] public string StudentId { get; set; } = string.Empty;
    [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("last_name")] public string LastName { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("timezone")] public string Timezone { get; set; } = string.Empty;
    [JsonPropertyName("grades")] public List<Grade>? Grades { get; set; }
}

public class Grade
{
    [JsonPropertyName("course_code")] public string CourseCode { get; set; } = string.Empty;
    [JsonPropertyName("term")] public string Term { get; set; } = string.Empty;
    [JsonPropertyName("grade")] public string GradeValue { get; set; } = string.Empty;
    [JsonPropertyName("credits")] public JsonElement Credits { get; set; } // Mixed type support
}

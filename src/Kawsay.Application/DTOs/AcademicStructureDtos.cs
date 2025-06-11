namespace Application.DTOs;

// --- Request DTOs for Creation ---

public record CreateCohortRequest(string Name, int TimetableId);
public record CreateStudentGroupRequest(string Name, int CohortId);
public record CreateSectionRequest(string Name, int StudentGroupId);
public record AssignStudentToSectionRequest(int StudentId, int SectionId);


// --- Response DTOs for Viewing ---

public class SectionDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<StudentDto> Students { get; set; } = [];
}

public class StudentGroupDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SectionDetailDto> Sections { get; set; } = [];
}

public class CohortDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TimetableId { get; set; }
    public List<StudentGroupDetailDto> StudentGroups { get; set; } = [];
}

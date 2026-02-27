namespace Application.DTOs;

public class GeneratedTimetableDto
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Score { get; set; }
    public List<GeneratedClassDto> Classes { get; set; } = [];
}

public class GeneratedClassDto
{
    public string TempId { get; set; } = string.Empty; // ID from solver (e.g. "REQ_101_1")
    public int RequirementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DayIndex { get; set; }
    public int StartPeriodIndex { get; set; }
    public int Duration { get; set; }
}
namespace Application.Models.Solver;

public class SchedulingResult
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long QualityScore { get; set; }
    public string Message { get; set; } = string.Empty;

    public List<ScheduledItem> ScheduledItems { get; set; } = [];
}

public class ScheduledItem
{
    public string ReferenceId { get; set; } = string.Empty; // ID used during scheduling (e.g., Requirement ID)
    public int DayIndex { get; set; }

    public int StartSlotIndex { get; set; }

    // Metadata useful for the UI immediately after generation
    public int Duration { get; set; }
}
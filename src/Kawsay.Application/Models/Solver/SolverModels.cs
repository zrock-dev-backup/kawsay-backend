namespace Application.Models.Solver;

// Domain models that mirror the Solver capabilities but are decoupled from Protobuf
public class SolverProblem
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public int Days { get; set; }
    public int SlotsPerDay { get; set; }
    public float MaxSolveTimeSeconds { get; set; } = 30f;
    public List<SolverTeacher> Teachers { get; set; } = [];
    public List<SolverGroup> Groups { get; set; } = [];
    public List<SolverActivity> Activities { get; set; } = [];
}

public class SolverTeacher
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SolverGroup
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SolverActivity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public List<string> GroupIds { get; set; } = [];

    public int Duration { get; set; }

// Metadata for mapping back
    public int OriginalRequirementId { get; set; }
}

public class SolverResult
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Optimal, Feasible, Infeasible
    public long QualityScore { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<SolverScheduledActivity> ScheduledActivities { get; set; } = [];
}

public class SolverScheduledActivity
{
    public string ActivityId { get; set; } = string.Empty;
    public int DayIndex { get; set; }
    public int StartSlotIndex { get; set; }
}
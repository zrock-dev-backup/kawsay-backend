namespace KawsayApiMockup.DTOs;

public class TimetableDay
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TimetablePeriod
{
    public int Id { get; set; }
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}

public class TimetableStructure
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TimetableDay> Days { get; set; } = new();
    public List<TimetablePeriod> Periods { get; set; } = new();
}

public class CreateTimetableRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Days { get; set; } = new();
    public List<CreateTimetablePeriodDto> Periods { get; set; } = new();
}

public class CreateTimetablePeriodDto
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}

public class UpdateTimetableRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;


    public List<string> Days { get; set; } = new();
    public List<CreateTimetablePeriodDto> Periods { get; set; } = new();
}
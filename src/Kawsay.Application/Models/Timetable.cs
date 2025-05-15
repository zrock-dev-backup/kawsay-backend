namespace Application.Models;

public class Timetable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Day> Days { get; set; } = new();
    public List<Period> Periods { get; set; } = new();
}
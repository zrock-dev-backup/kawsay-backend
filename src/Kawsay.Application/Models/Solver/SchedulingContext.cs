using Domain.Entities;

namespace Application.Models.Solver;
/// <summary>
/// Contains all the Domain information required to generate a schedule.
/// This decouples the Application layer from the specific format expected by the external solver.
/// </summary>
public class SchedulingContext
{
public string JobId { get; set; } = Guid.NewGuid().ToString();
public TimetableEntity Timetable { get; set; } = default!;
public List<CourseRequirementEntity> Requirements { get; set; } = [];
public List<CohortEntity> Cohorts { get; set; } = [];
}

using Domain.Entities;

namespace Application.Models.Solver;

/// <summary>
/// Contains all the Domain information required to generate a schedule.
/// Aligned with the Stage 1 & 2 Relational Mapping and Activity sets.
/// </summary>
public class SchedulingContext
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public TimetableEntity Timetable { get; set; } = default!;
    
    // Stage 2: Activities
    public List<CourseRequirementEntity> Requirements { get; set; } = [];
    
    // Stage 1: The 'Q' Set (Assigned Teachers)
    public List<string> AssignedTeacherIds { get; set; } = [];
    
    // Stage 1: Base constraints for A_T (Teacher Availability)
    public List<TeacherAvailabilityEntity> TeacherAvailabilities { get; set; } = [];
}

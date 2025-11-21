namespace Application.Features.Scheduling.Models;

/// <summary>
/// Represents the distilled scheduling attributes that heuristics need in order to
/// score a candidate slot. Keeping it separate from the persistence models allows
/// the heuristic layer to remain lightweight and easily testable.
/// </summary>
public sealed record EffectiveRequirementParameters
{
    /// <summary>
    /// Number of consecutive periods the requirement occupies.
    /// </summary>
    public required int Length { get; init; }
}


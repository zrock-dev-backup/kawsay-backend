using Application.Core;
using Application.Models.Solver;

namespace Application.Interfaces.Infrastructure;

public interface ISolverClient
{
    // CancellationToken added for industry-standard resilience
    Task<Result<SchedulingResult>> SolveAsync(SchedulingContext context, CancellationToken cancellationToken = default);
}

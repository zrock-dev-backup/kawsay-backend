using Application.Core;
using Application.DTOs;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Application.Models.Solver;

namespace Application.Services;

public class TimetableGenerationService(
    ITimetableRepository timetableRepo,
    ICourseRequirementRepository requirementRepo,
    IAcademicStructureRepository academicRepo,
    ISolverClient solverClient)
{
    public async Task<Result<GeneratedTimetableDto>> GenerateTimetableAsync(int timetableId, CancellationToken cancellationToken = default)
    {
        var timetable = await timetableRepo.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<GeneratedTimetableDto>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

        var requirements = await requirementRepo.GetByTimetableIdAsync(timetableId);
        var cohorts = await academicRepo.GetCohortsByTimetableAsync(timetableId);

        var context = new SchedulingContext
        {
            Timetable = timetable,
            Requirements = requirements.ToList(),
            Cohorts = cohorts
        };

        var result = await solverClient.SolveAsync(context, cancellationToken);
        if (result.IsFailure) return Result<GeneratedTimetableDto>.Failure(result.Error);

        var dto = new GeneratedTimetableDto
        {
            JobId = result.Value!.JobId,
            Status = result.Value.Status,
            Message = result.Value.Message,
            Score = result.Value.QualityScore,
            Classes = result.Value.ScheduledItems.Select(item => new GeneratedClassDto
            {
                TempId = item.ReferenceId,
                RequirementId = ParseReqId(item.ReferenceId),
                DayIndex = item.DayIndex,
                StartPeriodIndex = item.StartSlotIndex,
                Duration = item.Duration
            }).ToList()
        };

        return Result<GeneratedTimetableDto>.Success(dto);
    }

    private static int ParseReqId(string refId)
    {
        var parts = refId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var id)) return id;
        return 0;
    }
}

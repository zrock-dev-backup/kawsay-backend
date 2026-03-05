using Application.Core;
using Application.Interfaces.Persistence;
using Application.Models.Solver;
using Domain.Entities;

namespace Application.Services;

public class TimetableGenerationService(
    ITimetableRepository timetableRepo,
    ICourseRequirementRepository requirementRepo,
    IAcademicStructureRepository academicRepo,
    IStagedPlacementRepository stagedPlacementRepo, // <-- NEW: To save results
    IUnitOfWork unitOfWork, // <-- NEW: For transactional safety
    ISolverClient solverClient)
{
    public async Task<Result<GeneratedTimetableDto>> GenerateTimetableAsync(int timetableId,
        CancellationToken cancellationToken = default)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("GenerateTimetable");
        activity?.SetTag(KawsayTelemetry.Attributes.TimetableId, timetableId);

        var timetable = await timetableRepo.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<GeneratedTimetableDto>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

        using var dataLoadActivity = KawsayTelemetry.ActivitySource.StartActivity("HydrateSolverContext");
        var requirements = await requirementRepo.GetByTimetableIdAsync(timetableId);
        var cohorts = await academicRepo.GetCohortsByTimetableAsync(timetableId);

        var context = new SchedulingContext
        {
            Timetable = timetable,
            Requirements = requirements.ToList(),
            Cohorts = cohorts
        };

        activity?.SetTag(KawsayTelemetry.Attributes.ConstraintCount, context.Requirements.Count);

        // 1. Fire the ECU (gRPC Solver)
        var result = await solverClient.SolveAsync(context, cancellationToken);
        if (result.IsFailure)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, result.Error.Message);
            return Result<GeneratedTimetableDto>.Failure(result.Error);
        }

        activity?.SetTag(KawsayTelemetry.Attributes.JobId, result.Value!.JobId);
        activity?.SetTag(KawsayTelemetry.Attributes.GenerationStatus, result.Value!.Status);
        activity?.SetTag(KawsayTelemetry.Attributes.QualityScore, result.Value!.QualityScore);

        // 2. Persist the generated abstract grid back into physical database entities
        await unitOfWork.BeginTransactionAsync();
        try
        {
            using var dbActivity = KawsayTelemetry.ActivitySource.StartActivity("PersistResults");
            // Clear out any old staged placements for this specific timetable
            await stagedPlacementRepo.DeleteAllForTimetableAsync(timetableId);

            // Fetch deterministic mappings for Days and Periods 
            // (Assumes IDs are ordered chronologically just as they were built for the grid)
            var orderedDays = timetable.Days.OrderBy(d => d.Id).ToList();
            var orderedPeriods = timetable.Periods.OrderBy(p => p.Id).ToList();

            var dtoClasses = new List<GeneratedClassDto>();

            foreach (var item in result.Value!.ScheduledItems)
            {
                var reqId = ParseReqId(item.ReferenceId);

                // Bounds safety check
                if (item.DayIndex >= orderedDays.Count || item.StartSlotIndex >= orderedPeriods.Count)
                    continue;

                // Map Solver Matrix Index -> Database Primary Key
                var dayId = orderedDays[item.DayIndex].Id;
                var periodId = orderedPeriods[item.StartSlotIndex].Id;

                var placement = new StagedPlacementEntity
                {
                    CourseRequirementId = reqId,
                    DayId = dayId,
                    StartPeriodId = periodId,
                    Length = item.Duration
                };

                await stagedPlacementRepo.AddAsync(placement);

                dtoClasses.Add(new GeneratedClassDto
                {
                    TempId = item.ReferenceId,
                    RequirementId = reqId,
                    DayIndex = item.DayIndex,
                    StartPeriodIndex = item.StartSlotIndex,
                    Duration = item.Duration
                });
            }

            // Lock it in
            await unitOfWork.CommitTransactionAsync();

            var dto = new GeneratedTimetableDto
            {
                JobId = result.Value.JobId,
                Status = result.Value.Status,
                Message = result.Value.Message,
                Score = result.Value.QualityScore,
                Classes = dtoClasses
            };

            return Result<GeneratedTimetableDto>.Success(dto);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            return Result<GeneratedTimetableDto>.Failure(Error.Failure("Persistence.Failed",
                $"Failed to save generated schedule: {ex.Message}"));
        }
    }

    private static int ParseReqId(string refId)
    {
        var parts = refId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var id)) return id;
        return 0;
    }
}
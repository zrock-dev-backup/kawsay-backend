using Application.Core;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Application.Models.Solver;
using Domain.Entities;

namespace Application.Services;

public class TimetableGenerationService(
    ITimetableRepository timetableRepo,
    ICourseRequirementRepository requirementRepo,
    IRelationalMappingRepository mappingRepo,
    IStagedPlacementRepository stagedPlacementRepo,
    IUnitOfWork unitOfWork,
    ISolverClient solverClient)
{
    public async Task<Result<GeneratedTimetableDto>> GenerateTimetableAsync(int timetableId, CancellationToken cancellationToken = default)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("GenerateTimetable");
        activity?.SetTag(KawsayTelemetry.Attributes.TimetableId, timetableId);

        var timetable = await timetableRepo.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<GeneratedTimetableDto>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

        using var dataLoadActivity = KawsayTelemetry.ActivitySource.StartActivity("HydrateSolverContext");
        
        // Stage 2: Fetch Activities
        var requirements = await requirementRepo.GetByTimetableIdAsync(timetableId);
        
        // Stage 1: Fetch Q Set (Assigned Teachers) and A_T constraints
        var teacherIds = await mappingRepo.GetTeacherAssignmentsAsync(timetableId);
        var teacherAvailabilities = new List<TeacherAvailabilityEntity>();
        
        foreach (var tId in teacherIds)
        {
            var availabilities = await mappingRepo.GetTeacherAvailabilitiesAsync(timetableId, tId);
            teacherAvailabilities.AddRange(availabilities);
        }

        var context = new SchedulingContext
        {
            Timetable = timetable,
            Requirements = requirements,
            AssignedTeacherIds = teacherIds,
            TeacherAvailabilities = teacherAvailabilities
        };

        activity?.SetTag(KawsayTelemetry.Attributes.ConstraintCount, context.Requirements.Count);

        // Stage 3: Fire the ECU (gRPC Solver)
        var result = await solverClient.SolveAsync(context, cancellationToken);
        if (result.IsFailure)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, result.Error.Message);
            return Result<GeneratedTimetableDto>.Failure(result.Error);
        }

        activity?.SetTag(KawsayTelemetry.Attributes.JobId, result.Value!.JobId);
        activity?.SetTag(KawsayTelemetry.Attributes.GenerationStatus, result.Value!.Status);
        activity?.SetTag(KawsayTelemetry.Attributes.QualityScore, result.Value!.QualityScore);

        // Persist the generated abstract grid back into physical database entities
        await unitOfWork.BeginTransactionAsync();
        try
        {
            using var dbActivity = KawsayTelemetry.ActivitySource.StartActivity("PersistResults");
            
            await stagedPlacementRepo.DeleteAllForTimetableAsync(timetableId);

            var orderedDays = timetable.Days.OrderBy(d => d.Id).ToList();
            var orderedPeriods = timetable.Periods.OrderBy(p => p.Id).ToList();
            var dtoClasses = new List<GeneratedClassDto>();

            foreach (var item in result.Value!.ScheduledItems)
            {
                var reqId = ParseReqId(item.ReferenceId);
                
                if (item.DayIndex >= orderedDays.Count || item.StartSlotIndex >= orderedPeriods.Count)
                    continue;

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
            return Result<GeneratedTimetableDto>.Failure(Error.Failure("Persistence.Failed", $"Failed to save generated schedule: {ex.Message}"));
        }
    }

    private static int ParseReqId(string refId)
    {
        var parts = refId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var id)) return id;
        return 0;
    }
}

// Supporting DTOs for the return signature
public class GeneratedTimetableDto
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Score { get; set; }
    public List<GeneratedClassDto> Classes { get; set; } = [];
}

public class GeneratedClassDto
{
    public string TempId { get; set; } = string.Empty;
    public int RequirementId { get; set; }
    public int DayIndex { get; set; }
    public int StartPeriodIndex { get; set; }
    public int Duration { get; set; }
}

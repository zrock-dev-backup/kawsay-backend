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
        var teacherIds = await mappingRepo.GetTeacherAssignmentsAsync(timetableId);
        var teacherAvailabilities = new List<TeacherAvailabilityEntity>();

        foreach (var tId in teacherIds)
        {
            var availabilities = await mappingRepo.GetTeacherAvailabilitiesAsync(timetableId, tId);
            teacherAvailabilities.AddRange(availabilities);
        }

        var context = new SchedulingContext
        {
            JobId = Guid.NewGuid().ToString(),
            Timetable = timetable,
            Requirements = requirements,
            AssignedTeacherIds = teacherIds,
            TeacherAvailabilities = teacherAvailabilities
        };

        activity?.SetTag(KawsayTelemetry.Attributes.ConstraintCount, context.Requirements.Count);

        var result = await solverClient.SolveAsync(context, cancellationToken);
        if (result.IsFailure)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, result.Error.Message);
            return Result<GeneratedTimetableDto>.Failure(result.Error);
        }

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
                if (item.DayIndex >= orderedDays.Count || item.StartSlotIndex >= orderedPeriods.Count) continue;

                var dayId = orderedDays[item.DayIndex].Id;
                var periodId = orderedPeriods[item.StartSlotIndex].Id;

                await stagedPlacementRepo.AddAsync(new StagedPlacementEntity
                {
                    CourseRequirementId = reqId,
                    DayId = dayId,
                    StartPeriodId = periodId,
                    Length = item.Duration
                });

                dtoClasses.Add(new GeneratedClassDto
                {
                    TempId = item.ReferenceId, RequirementId = reqId, DayIndex = item.DayIndex,
                    StartPeriodIndex = item.StartSlotIndex, Duration = item.Duration
                });
            }

            await unitOfWork.CommitTransactionAsync();

            return Result<GeneratedTimetableDto>.Success(new GeneratedTimetableDto
            {
                JobId = result.Value.JobId,
                Status = result.Value.Status,
                Message = result.Value.Message,
                Score = result.Value.QualityScore,
                Classes = dtoClasses
            });
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result<GeneratedTimetableDto>.Failure(Error.Failure("Persistence.Failed",
                $"Failed to save schedule: {ex.Message}"));
        }
    }

    private static int ParseReqId(string refId)
    {
        var parts = refId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var id)) return id;
        return 0;
    }
}

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
using Application.Core;
using Application.Interfaces.Infrastructure;
using Application.Models.Solver;
using Grpc.Core;
using Infrastructure.Protos;
using Microsoft.Extensions.Logging;

namespace Infrastructure.External;

public class SolverGrpcClient(
    TimetablingService.TimetablingServiceClient grpcClient,
    ILogger<SolverGrpcClient> logger) : ISolverClient
{
    public async Task<Result<SchedulingResult>> SolveAsync(SchedulingContext context, CancellationToken cancellationToken = default)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("Grpc.DispatchSolver");
        activity?.SetTag(KawsayTelemetry.Attributes.JobId, context.JobId);

        try
        {
            var request = MapDomainToProto(context);
            
            activity?.SetTag(KawsayTelemetry.Attributes.ActivityCount, request.Activities.Count);
            activity?.SetTag("solver.raw.teachers_count", request.Teachers.Count);
            activity?.SetTag("solver.raw.groups_count", request.StudentGroups.Count);
            activity?.SetTag("solver.config.max_time", request.Config.MaxSolveTimeSeconds);

            logger.LogInformation("Sending grpc request for Job {JobId}. Activities: {Count}", context.JobId, request.Activities.Count);
            
            var response = await grpcClient.SolveAsync(request, cancellationToken: cancellationToken);
            
            activity?.SetTag("solver.response.status", response.Status.ToString());
            
            return Result<SchedulingResult>.Success(MapProtoToDomain(response, request));
        }
        catch (RpcException ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Status.Detail);
            logger.LogError(ex, "gRPC Call failed for Job {JobId}. Status: {Status}", context.JobId, ex.Status.StatusCode);
            return Result<SchedulingResult>.Failure(Error.Failure("Solver.ConnectionError", $"gRPC connection failed: {ex.Status.Detail}"));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Solver failed for Job {JobId}", context.JobId);
            return Result<SchedulingResult>.Failure(Error.Failure("Solver.InternalError", ex.Message));
        }
    }

    private static ProblemDefinition MapDomainToProto(SchedulingContext context)
    {
        var proto = new ProblemDefinition
        {
            JobId = context.JobId,
            Config = new SolverConfig { MaxSolveTimeSeconds = 10f }, // Could be config-driven
            TimeGrid = new TimeGrid
            {
                Days = context.Timetable.Days.Count > 0 ? context.Timetable.Days.Count : 5,
                SlotsPerDay = context.Timetable.Periods.Count > 0 ? context.Timetable.Periods.Count : 10
            }
        };

        // Deterministic ordering to map Database IDs to 0-based Solver Grid Indices
        var orderedDays = context.Timetable.Days.OrderBy(d => d.Id).ToList();
        var orderedPeriods = context.Timetable.Periods.OrderBy(p => p.Id).ToList();

        // 1. Map Teachers & Availabilities (A_T)
        var hardConstraints = context.TeacherAvailabilities
            .Where(a => a.Level == ConstraintLevel.Hard)
            .ToList();

        foreach (var tId in context.AssignedTeacherIds.Distinct())
        {
            var pTeacher = new Teacher { Id = tId, Name = $"Teacher {tId}" };
            
            // Map hard constraints to unavailable slots
            var tConstraints = hardConstraints.Where(c => c.TeacherId == tId);
            foreach(var c in tConstraints)
            {
                var dayIndex = orderedDays.FindIndex(d => d.Id == c.DayId);
                var periodIndex = orderedPeriods.FindIndex(p => p.Id == c.PeriodId);
                
                if (dayIndex >= 0 && periodIndex >= 0)
                {
                    pTeacher.UnavailableSlots.Add(new TimeSlot { DayIndex = dayIndex, SlotIndex = periodIndex });
                }
            }
            
            proto.Teachers.Add(pTeacher);
        }

        // 2. Map Student Groups
        var groupIds = context.Requirements
            .Where(r => !string.IsNullOrEmpty(r.StudentGroupId))
            .Select(r => r.StudentGroupId!)
            .Distinct();

        foreach (var gId in groupIds)
        {
            proto.StudentGroups.Add(new StudentGroup { Id = gId, Name = $"Group {gId}" });
        }

        // 3. Map Activities (Explode Requirements)
        foreach (var req in context.Requirements)
        {
            // Business Logic: If frequency is 2, we generate 2 distinct activities for the solver
            for (int i = 0; i < req.FrequencyPerWeek; i++)
            {
                var activityId = $"REQ_{req.Id}_{i}";
                var pActivity = new Activity
                {
                    Id = activityId,
                    Name = req.SubjectId, 
                    TeacherId = req.TeacherId ?? "",
                    DurationInSlots = req.DurationInPeriods
                };

                if (!string.IsNullOrEmpty(req.StudentGroupId))
                {
                    pActivity.StudentGroupIds.Add(req.StudentGroupId);
                }

                proto.Activities.Add(pActivity);
            }
        }

        return proto;
    }

    private static SchedulingResult MapProtoToDomain(Solution proto, ProblemDefinition request)
    {
        var result = new SchedulingResult
        {
            JobId = proto.JobId,
            Status = proto.Status.ToString(),
            QualityScore = proto.QualityScore,
            Message = proto.Message
        };

        var durationMap = request.Activities.ToDictionary(a => a.Id, a => a.DurationInSlots);

        foreach (var item in proto.ScheduledActivities)
        {
            result.ScheduledItems.Add(new ScheduledItem
            {
                ReferenceId = item.ActivityId,
                DayIndex = item.StartTime.DayIndex,
                StartSlotIndex = item.StartTime.SlotIndex,
                Duration = durationMap.TryGetValue(item.ActivityId, out var d) ? d : 1
            });
        }

        return result;
    }
}

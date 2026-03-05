using Application.Core;
using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;
namespace Application.Services;

public class Stage2ActivityService(ICourseRequirementRepository reqRepo, IUnitOfWork unitOfWork)
{
    public async Task<Result<int>> CreateActivityAsync(int timetableId, CourseRequirementCreateDto dto)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("Stage2.CreateActivity");
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = new CourseRequirementEntity
            {
                TimetableId = timetableId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                StudentGroupId = dto.StudentGroupId,
                DurationInPeriods = dto.DurationInPeriods,
                FrequencyPerWeek = dto.FrequencyPerWeek,
                Priority = dto.Priority,
                ClassType = dto.ClassType
            };
            await reqRepo.AddAsync(entity);
            await unitOfWork.CommitTransactionAsync();
            return Result<int>.Success(entity.Id);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            return Result<int>.Failure(Error.Failure("Stage2.Failed", ex.Message));
        }
    }
}
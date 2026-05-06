using Application.Core;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class Stage1RelationalMappingService(
    ITimetableRepository timetableRepo,
    IRelationalMappingRepository mappingRepo,
    IUnitOfWork unitOfWork)
{

    // --- TEACHER AVAILABILITY ---

    public async Task<Result> UpdateTeacherAvailabilityAsync(int timetableId, string teacherId, int dayId, int periodId,
        int weightPercentage)
    {
        if (weightPercentage is < 0 or > 100)
            return Result.Failure(Error.Validation("Validation.InvalidWeight",
                "Weight percentage must be between 0 and 100."));

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = await mappingRepo.GetTeacherAvailabilityAsync(timetableId, teacherId, dayId, periodId);
            if (entity == null)
                return Result.Failure(Error.NotFound("NotFound.Constraint",
                    "Teacher availability constraint not found."));

            entity.WeightPercentage = weightPercentage;
            await unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result.Failure(Error.Failure("Stage1.Failed", ex.Message));
        }
    }

    public async Task<Result> RemoveTeacherAvailabilityAsync(int timetableId, string teacherId, int dayId, int periodId)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = await mappingRepo.GetTeacherAvailabilityAsync(timetableId, teacherId, dayId, periodId);
            if (entity == null)
                return Result.Failure(Error.NotFound("NotFound.Constraint",
                    "Teacher availability constraint not found."));

            mappingRepo.RemoveTeacherAvailability(entity);
            await unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result.Failure(Error.Failure("Stage1.Failed", ex.Message));
        }
    }

    public async Task<Result> UpdateStudentAvailabilityAsync(int timetableId, string studentId, int dayId, int periodId,
        int weightPercentage)
    {
        if (weightPercentage is < 0 or > 100)
            return Result.Failure(Error.Validation("Validation.InvalidWeight",
                "Weight percentage must be between 0 and 100."));

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = await mappingRepo.GetStudentAvailabilityAsync(timetableId, studentId, dayId, periodId);
            if (entity == null)
                return Result.Failure(Error.NotFound("NotFound.Constraint",
                    "Student availability constraint not found."));

            entity.WeightPercentage = weightPercentage;
            await unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result.Failure(Error.Failure("Stage1.Failed", ex.Message));
        }
    }

    public async Task<Result> RemoveStudentAvailabilityAsync(int timetableId, string studentId, int dayId, int periodId)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var entity = await mappingRepo.GetStudentAvailabilityAsync(timetableId, studentId, dayId, periodId);
            if (entity == null)
                return Result.Failure(Error.NotFound("NotFound.Constraint",
                    "Student availability constraint not found."));

            mappingRepo.RemoveStudentAvailability(entity);
            await unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result.Failure(Error.Failure("Stage1.Failed", ex.Message));
        }
    }
}
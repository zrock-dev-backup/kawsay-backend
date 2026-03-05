using Application.Core;
using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
namespace Application.Services;
public class Stage1RelationalMappingService(
ITimetableRepository timetableRepo,
IRelationalMappingRepository mappingRepo,
IUnitOfWork unitOfWork)
{
// A_T Calculation: Returns all grid slots EXCEPT Hard constraints for the specific teacher
public async Task<Result<List<TimeSlotDto>>> CalculateTeacherAvailabilityMatrixAsync(int timetableId, string teacherId)
{
using var activity = KawsayTelemetry.ActivitySource.StartActivity("Stage1.Calculate_AT");
var timetable = await timetableRepo.GetByIdAsync(timetableId);
    if (timetable == null) return Result<List<TimeSlotDto>>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

    var constraints = await mappingRepo.GetTeacherAvailabilitiesAsync(timetableId, teacherId);
    var hardConstraints = constraints.Where(c => c.Level == ConstraintLevel.Hard).ToList();

    var availableSlots = new List<TimeSlotDto>();

    foreach (var day in timetable.Days)
    {
        foreach (var period in timetable.Periods)
        {
            if (!hardConstraints.Any(c => c.DayId == day.Id && c.PeriodId == period.Id))
            {
                availableSlots.Add(new TimeSlotDto(day.Id, period.Id));
            }
        }
    }

    return Result<List<TimeSlotDto>>.Success(availableSlots);
}

// A_S Calculation: Returns all grid slots EXCEPT Hard constraints for the specific student
public async Task<Result<List<TimeSlotDto>>> CalculateStudentAvailabilityMatrixAsync(int timetableId, string studentId)
{
    using var activity = KawsayTelemetry.ActivitySource.StartActivity("Stage1.Calculate_AS");
    
    var timetable = await timetableRepo.GetByIdAsync(timetableId);
    if (timetable == null) return Result<List<TimeSlotDto>>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

    var constraints = await mappingRepo.GetStudentAvailabilitiesAsync(timetableId, studentId);
    var hardConstraints = constraints.Where(c => c.Level == ConstraintLevel.Hard).ToList();

    var availableSlots = new List<TimeSlotDto>();

    foreach (var day in timetable.Days)
    {
        foreach (var period in timetable.Periods)
        {
            if (!hardConstraints.Any(c => c.DayId == day.Id && c.PeriodId == period.Id))
            {
                availableSlots.Add(new TimeSlotDto(day.Id, period.Id));
            }
        }
    }

    return Result<List<TimeSlotDto>>.Success(availableSlots);
}

public async Task<Result> AddTeacherAvailabilityAsync(int timetableId, TeacherAvailabilityDto dto)
{
    await unitOfWork.BeginTransactionAsync();
    try
    {
        await mappingRepo.AddTeacherAvailabilityAsync(new TeacherAvailabilityEntity
        {
            TimetableId = timetableId,
            TeacherId = dto.TeacherId,
            DayId = dto.DayId,
            PeriodId = dto.PeriodId,
            Level = dto.Level
        });
        await unitOfWork.CommitTransactionAsync();
        return Result.Success();
    }
    catch (Exception ex)
    {
        await unitOfWork.RollbackTransactionAsync();
        return Result.Failure(Error.Failure("Stage1.Failed", ex.Message));
    }
}

public async Task<Result> AddStudentEnrollmentAsync(int timetableId, StudentEnrollmentDto dto)
{
    await unitOfWork.BeginTransactionAsync();
    try
    {
        await mappingRepo.AddStudentEnrollmentAsync(new StudentEnrollmentEntity
        {
            TimetableId = timetableId,
            StudentId = dto.StudentId,
            SubjectId = dto.SubjectId
        });
        await unitOfWork.CommitTransactionAsync();
        return Result.Success();
    }
    catch (Exception ex)
    {
        await unitOfWork.RollbackTransactionAsync();
        return Result.Failure(Error.Failure("Stage1.Failed", ex.Message));
    }
}

public async Task<Result> AddDeferredStudentAsync(int timetableId, DeferredStudentDto dto)
{
    await unitOfWork.BeginTransactionAsync();
    try
    {
        await mappingRepo.AddDeferredStudentAsync(new DeferredStudentEntity
        {
            TimetableId = timetableId,
            StudentId = dto.StudentId,
            Reason = dto.Reason
        });
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

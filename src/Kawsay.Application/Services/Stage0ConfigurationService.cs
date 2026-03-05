using Application.Core;
using Application.Interfaces.Persistence;
using Domain.Entities;
namespace Application.Services;

public class Stage0ConfigurationService(ITimetableRepository timetableRepo, IUnitOfWork unitOfWork)
{
    public async Task<Result<int>> CreateTimetableAsync(TimetableCreateDto dto)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("Stage0.CreateTimetable");
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var timetable = new TimetableEntity
            {
                Name = dto.Name,
                Timezone = dto.Timezone,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Days = dto.Days.Select(d => new TimetableDayEntity { Name = d }).ToList(),
                Periods = dto.Periods.Select(p => new TimetablePeriodEntity { StartTime = p.Start, EndTime = p.End })
                    .ToList(),
                SelectedSubjects = dto.SubjectIds.Select(s => new SubjectSelectionEntity { SubjectId = s }).ToList()
            };
            await timetableRepo.AddAsync(timetable);
            await unitOfWork.CommitTransactionAsync();

            return Result<int>.Success(timetable.Id);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            return Result<int>.Failure(Error.Failure("Stage0.Failed", ex.Message));
        }
    }
}
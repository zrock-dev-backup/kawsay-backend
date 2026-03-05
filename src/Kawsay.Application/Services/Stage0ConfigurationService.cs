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
                Periods = dto.Periods.Select(p => new TimetablePeriodEntity { StartTime = p.Start, EndTime = p.End }).ToList(),
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

    public async Task<Result<List<TimetableSummaryDto>>> GetAllTimetablesAsync()
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("Stage0.GetAllTimetables");
        
        var timetables = await timetableRepo.GetAllAsync();
        
        // Map to summary (avoids over-fetching complex relational data for lists)
        var dtos = timetables.Select(t => new TimetableSummaryDto(
            t.Id, t.Name, t.Timezone, t.StartDate, t.EndDate
        )).ToList();

        return Result<List<TimetableSummaryDto>>.Success(dtos);
    }

    public async Task<Result<TimetableDto>> GetTimetableByIdAsync(int id)
    {
        using var activity = KawsayTelemetry.ActivitySource.StartActivity("Stage0.GetTimetableById");
        activity?.SetTag(KawsayTelemetry.Attributes.TimetableId, id);

        var timetable = await timetableRepo.GetByIdAsync(id);
        
        if (timetable == null)
            return Result<TimetableDto>.Failure(Error.NotFound("Timetable.NotFound", $"Timetable with ID {id} was not found."));

        // Map complete entity including relationships populated by the repository's .Include() calls
        var dto = new TimetableDto(
            timetable.Id,
            timetable.Name,
            timetable.Timezone,
            timetable.StartDate,
            timetable.EndDate,
            timetable.Days.Select(d => d.Name).ToList(),
            timetable.Periods.Select(p => new PeriodDto(p.StartTime, p.EndTime)).ToList(),
            timetable.SelectedSubjects.Select(s => s.SubjectId).ToList()
        );

        return Result<TimetableDto>.Success(dto);
    }
}

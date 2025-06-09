using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class TimetableService(ITimetableRepository repository)
{
    public async Task<Timetable?> GetByIdAsync(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        return entity == null
            ? null
            : new Timetable
            {
                Id = entity.Id,
                Name = entity.Name,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Days = entity.Days.Select(timetableDayEntity => new Day
                    {
                        Id = timetableDayEntity.Id,
                        Name = timetableDayEntity.Name
                    })
                    .ToList(),

                Periods = entity.Periods.Select(timetablePeriodEntity => new Period
                    {
                        Id = timetablePeriodEntity.Id,
                        Start = timetablePeriodEntity.Start,
                        End = timetablePeriodEntity.End,
                    })
                    .ToList()
            };
    }

    public async Task<IEnumerable<Timetable>> GetAllAsync()
    {
        var entities = await repository.GetAllAsync();
        return entities.Select(e => new Timetable
        {
            Id = e.Id,
            Name = e.Name,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
        });
    }

    public async Task<Timetable> CreateTimetableAsync(Timetable timetable)
    {
        var entity = new TimetableEntity
        {
            Id = timetable.Id,
            Name = timetable.Name,
            StartDate = timetable.StartDate,
            EndDate = timetable.EndDate,
            Days = timetable.Days.Select(day => new TimetableDayEntity
            {
                Name = day.Name,
            }).ToList(),
            Periods = timetable.Periods.Select(period => new TimetablePeriodEntity
            {
                Start = period.Start,
                End = period.End,
            }).ToList(),
        };
        var createdEntity = await repository.AddAsync(entity);
        return new Timetable
        {
            Id = createdEntity.Id,
            Name = createdEntity.Name,
            StartDate = createdEntity.StartDate,
            EndDate = createdEntity.EndDate,
            Days = createdEntity.Days.Select(timetableDayEntity => new Day
                {
                    Id = timetableDayEntity.Id,
                    Name = timetableDayEntity.Name
                })
                .ToList(),

            Periods = createdEntity.Periods.Select(timetablePeriodEntity => new Period
                {
                    Id = timetablePeriodEntity.Id,
                    Start = timetablePeriodEntity.Start,
                    End = timetablePeriodEntity.End,
                })
                .ToList()
        };
    }
}
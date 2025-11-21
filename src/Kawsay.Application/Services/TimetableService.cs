using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class TimetableService(ITimetableRepository repository)
{
    public async Task<Timetable?> GetByIdAsync(int id)
    {
        var entity = await repository.GetByIdAsync(id);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<Timetable>> GetAllAsync()
    {
        var entities = await repository.GetAllAsync();
        return entities.Select(Map);
    }

    public async Task<Timetable?> PublishTimetableAsync(int id)
    {
        // NOTE: Publishing semantics are not defined yet. For now this simply
        // returns the latest persisted representation so the frontend can proceed.
        var entity = await repository.GetByIdAsync(id);
        return entity == null ? null : Map(entity);
    }

    public async Task<Timetable?> GetMasterTimetableAsync()
    {
        var entities = await repository.GetAllAsync();
        var master = entities
            .OrderByDescending(e => e.EndDate)
            .ThenByDescending(e => e.Id)
            .FirstOrDefault();

        return master == null ? null : Map(master);
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
        return Map(createdEntity);
    }

    private static Timetable Map(TimetableEntity entity)
    {
        return new Timetable
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Days = entity.Days.Select(day => new Day
                {
                    Id = day.Id,
                    Name = day.Name
                })
                .ToList(),
            Periods = entity.Periods.Select(period => new Period
                {
                    Id = period.Id,
                    Start = period.Start,
                    End = period.End
                })
                .ToList()
        };
    }
}
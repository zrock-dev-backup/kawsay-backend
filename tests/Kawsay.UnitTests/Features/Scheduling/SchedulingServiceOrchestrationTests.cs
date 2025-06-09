using Application.Features.Scheduling;
using Application.Interfaces.Persistence;
using Application.Models;
using Application.Services;
using Domain.Entities;
using NSubstitute;

namespace Kawsay.UnitTests.Features.Scheduling;

public class SchedulingServiceOrchestrationTests
{
    private readonly ITimetableRepository _timetableRepo = Substitute.For<ITimetableRepository>();
    private readonly IClassRepository _classRepo = Substitute.For<IClassRepository>();
    private readonly ITeacherRepository _teacherRepo = Substitute.For<ITeacherRepository>();
    private readonly IClassOccurrenceRepository _occurrenceRepo = Substitute.For<IClassOccurrenceRepository>();
    private readonly CalendarizationService _calendarizer = Substitute.For<CalendarizationService>();

    private readonly SchedulingService _sut;

    public SchedulingServiceOrchestrationTests()
    {
        _sut = new SchedulingService(_timetableRepo, _classRepo, _teacherRepo, _occurrenceRepo, _calendarizer);
    }

    [Fact]
    public async Task GenerateScheduleAsync_WhenSolverSucceeds_CallsCalendarizationAndSavesResult()
    {
        // Arrange
        const int timetableId = 1;
        var timetable = new TimetableEntity
        {
            Id = timetableId,
            Name = "Test Timetable",
            StartDate = new DateOnly(2024, 10, 28),
            EndDate = new DateOnly(2024, 11, 1),
            Days = [new TimetableDayEntity { Id = 1, Name = "Monday" }],
            Periods = [new TimetablePeriodEntity { Id = 101, Start = "08:00", End = "09:00" }]
        };
        
        var teacherEntity = new TeacherEntity { Id = 1, Name = "Test Teacher", Type = "Professor" };
        var courseEntity = new CourseEntity { Id = 1, Name = "Test Course", Code = "T101" };

        var classEntity = new ClassEntity
        {
            Id = 1,
            TimetableId = timetableId,
            Timetable = timetable,
            Frequency = 1,
            Length = 1,
            CourseId = courseEntity.Id,
            Course = courseEntity,
            TeacherId = teacherEntity.Id,
            Teacher = teacherEntity,
            PeriodPreferences =
            [
                new PeriodPreferenceEntity
                {
                    StartPeriodId = 101
                }
            ],
        };

        _timetableRepo.GetByIdAsync(timetableId).Returns(timetable);
        _classRepo.GetAllAsync(timetableId).Returns([classEntity]);
        _teacherRepo.GetAllAsync().Returns([new TeacherEntity { Id = 1, Name = "Test Teacher" }]);

        var projectedOccurrences = new List<ClassOccurrenceEntity>
        {
            new() { ClassId = 1, Date = new DateOnly(2024, 10, 28), StartPeriodId = 101 }
        };

        // Mock the CalendarizationService to return a predefined result
        _calendarizer.ProjectSchedule(Arg.Any<TimetableEntity>(), Arg.Any<List<AbstractClassSchedule>>())
            .Returns(projectedOccurrences);

        // Act
        var success = await _sut.GenerateScheduleAsync(timetableId);

        // Assert
        Assert.True(success);

        // Verify that the repository was called to save the projected results
        await _occurrenceRepo.Received(1).AddRangeAsync(projectedOccurrences);
        await _occurrenceRepo.Received(1)
            .DeleteByClassIdAsync(Arg.Is<List<int>>(ids => ids.SequenceEqual(new[] { 1 })));
    }
}
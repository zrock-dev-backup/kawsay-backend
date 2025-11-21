using Application.Features.Scheduling;
using Application.Features.Scheduling.Models;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Kawsay.Domain.Enums;
using Kawsay.Domain.ValueObjects;
using NSubstitute;

namespace Kawsay.UnitTests.Application.Features.Scheduling
{
    public class SchedulingEngineServiceTests
    {
        private readonly ICourseRequirementRepository _courseRequirementRepo;
        private readonly IAvailabilityReadModelRepository _availabilityRepo;
        private readonly IStagedPlacementRepository _stagedPlacementRepo;
        private readonly SchedulingEngineService _sut;

        public SchedulingEngineServiceTests()
        {
            _courseRequirementRepo = Substitute.For<ICourseRequirementRepository>();
            _availabilityRepo = Substitute.For<IAvailabilityReadModelRepository>();
            _stagedPlacementRepo = Substitute.For<IStagedPlacementRepository>();
            _sut = new SchedulingEngineService(_courseRequirementRepo, _availabilityRepo, _stagedPlacementRepo);
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_NoConflicts_ShouldReturnAllPossibleSlots()
        {
            // Arrange
            var timetable = CreateTimetable(1, 3, 5);
            var courseRequirement = CreateRequirement(timetable, 2);
            var matrices = CreateEmptyMatrices(timetable, courseRequirement);

            SetupMocks(timetable, courseRequirement, matrices, []);

            // Act
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(courseRequirement.Id);

            // Assert
            Assert.Equal(12, validSlots.Count); // 3 days * 3 possible starts (0,1,2 for periods 0-4 with length 2)
            Assert.All(validSlots, slot => Assert.Equal(SlotType.Viable, slot.Type)); // No preferences
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_WithConflicts_ShouldExcludeConflictingSlots()
        {
            // Arrange
            var timetable = CreateTimetable(1, 3, 5);
            var requirement = CreateRequirement(timetable, 2);
            var matrices = CreateMatricesWithConflicts(timetable, requirement);

            SetupMocks(timetable, requirement, matrices, new List<StagedPlacement>());

            // Act
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(requirement.Id);

            // Assert
            // Expected: Day 1: starts at 0,3; Day 2: starts at 3; Day 3: starts at 0,1,2
            Assert.Equal(6, validSlots.Count);
            var expectedStarts = new Dictionary<int, List<int>>
            {
                { 1, new List<int> { 10, 13 } },
                { 2, new List<int> { 13 } },
                { 3, new List<int> { 10, 11, 12 } }
            };
            foreach (var day in expectedStarts)
            {
                var slotsForDay = validSlots.Where(s => s.DayId == day.Key).Select(s => s.StartPeriodId).ToList();
                Assert.Equal(day.Value, slotsForDay);
            }
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_LengthOne_ShouldReturnAllAvailablePeriods()
        {
            // Arrange
            var timetable = CreateTimetable(1, 1, 3);
            var requirement = CreateRequirement(timetable, 1);
            var matrices = CreateEmptyMatrices(timetable, requirement);

            SetupMocks(timetable, requirement, matrices, new List<StagedPlacement>());

            // Act
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(requirement.Id);

            // Assert
            Assert.Equal(3, validSlots.Count); // 1 day * 3 periods
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_LengthEqualToPeriods_ShouldReturnOneSlotPerDayIfAvailable()
        {
            // Arrange
            var timetable = CreateTimetable(1, 2, 3);
            var requirement = CreateRequirement(timetable, 3);
            var matrices = CreateEmptyMatrices(timetable, requirement);

            SetupMocks(timetable, requirement, matrices, new List<StagedPlacement>());

            // Act
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(requirement.Id);

            // Assert
            Assert.Equal(2, validSlots.Count); // 2 days, each with one possible slot starting at period 0
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_LengthGreaterThanPeriods_ShouldReturnNoSlots()
        {
            // Arrange
            var timetable = CreateTimetable(1, 2, 3);
            var requirement = CreateRequirement(timetable, 4);
            var matrices = CreateEmptyMatrices(timetable, requirement);

            SetupMocks(timetable, requirement, matrices, new List<StagedPlacement>());

            // Act
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(requirement.Id);

            // Assert
            Assert.Empty(validSlots);
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_WithStagedPlacements_ShouldConsiderAsConflicts()
        {
            // Arrange
            var timetable = CreateTimetable(1, 1, 3);
            var requirement = CreateRequirement(timetable, 1);
            var matrices = CreateEmptyMatrices(timetable, requirement);
            var stagedPlacements = new List<StagedPlacement>
                { new StagedPlacement(99, 1, 10, 1, new List<int> { 101 }) };

            SetupMocks(timetable, requirement, matrices, stagedPlacements);

            // Act
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(requirement.Id);

            // Assert
            Assert.Equal(2,
                validSlots.Count); // Periods 11 and 12 should be available, 10 is conflicted due to staged placement
            Assert.DoesNotContain(validSlots, slot => slot.StartPeriodId == 10);
        }

        private TimetableEntity CreateTimetable(int id, int dayCount, int periodCount)
        {
            var days = Enumerable.Range(1, dayCount).Select(i => new TimetableDayEntity { Id = i, Name = $"Day{i}" })
                .ToList();
            var periods = Enumerable.Range(10, periodCount)
                .Select(i => new TimetablePeriodEntity { Id = i, Start = $"{i}:00" }).ToList();
            return new TimetableEntity { Id = id, Days = days, Periods = periods };
        }

        private CourseRequirementEntity CreateRequirement(TimetableEntity timetable, int length)
        {
            return new CourseRequirementEntity
            {
                Id = 1,
                Timetable = timetable,
                PreferredTeacherId = 101,
                StudentGroupId = 201,
                DurationInPeriods = length,
                FrequencyPerWeek = 1,
                TimetableId = timetable.Id,
                CourseId = 42,
                Course = new CourseEntity { Id = 42, Name = "Course" },
                Priority = CourseRequirementPriority.High,
                EffectiveDateRange = new DateRange(DateOnly.FromDateTime(DateTime.UtcNow),
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
                Status = CourseRequirementStatus.Pending
            };
        }

        private Dictionary<int, SchedulingMatrix> CreateEmptyMatrices(TimetableEntity timetable,
            CourseRequirementEntity requirement)
        {
            var matrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
            return new Dictionary<int, SchedulingMatrix>
            {
                { requirement.PreferredTeacherId!.Value, matrix },
                { requirement.StudentGroupId.Value, matrix }
            };
        }

        private Dictionary<int, SchedulingMatrix> CreateMatricesWithConflicts(TimetableEntity timetable,
            CourseRequirementEntity requirement)
        {
            var teacherMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
            teacherMatrix.Set(0, 2, 1); // Day 1, Period 12
            teacherMatrix.Set(2, 4, 1); // Day 3, Period 14
            var groupMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
            groupMatrix.Set(1, 1, 1); // Day 2, Period 11
            groupMatrix.Set(1, 2, 1); // Day 2, Period 12
            return new Dictionary<int, SchedulingMatrix>
            {
                { requirement.PreferredTeacherId!.Value, teacherMatrix },
                { requirement.StudentGroupId.Value, groupMatrix }
            };
        }

        private void SetupMocks(TimetableEntity timetable, CourseRequirementEntity requirement,
            Dictionary<int, SchedulingMatrix> matrices, List<StagedPlacement> stagedPlacements)
        {
            _courseRequirementRepo.GetByIdAsync(requirement.Id)!.Returns(Task.FromResult<CourseRequirementEntity?>(requirement));
            _availabilityRepo.GetMatricesForResourcesAsync(Arg.Any<List<int>>()).Returns(Task.FromResult(matrices));
            _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(timetable.Id)
                .Returns(Task.FromResult(stagedPlacements));
        }
    }
}
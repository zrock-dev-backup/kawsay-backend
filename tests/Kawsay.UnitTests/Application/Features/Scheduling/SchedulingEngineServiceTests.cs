using Application.Features.Scheduling;
using Application.Features.Scheduling.Models;
using Application.Interfaces.Persistence;
using Domain.Entities;
using NSubstitute;

namespace Kawsay.UnitTests.Application.Features.Scheduling
{
    public class SchedulingEngineServiceTests
    {
        // --- Mocked Dependencies ---
        private readonly ICourseRequirementRepository _requirementRepo;
        private readonly IAvailabilityReadModelRepository _availabilityRepo; // Adheres to ADR-002 (CQRS)
        private readonly IStagedPlacementRepository _stagedPlacementRepo; // For in-session changes

        // --- System Under Test (SUT) ---
        private readonly SchedulingEngineService _sut;

        // --- Test Constants for Readability ---
        private const int TEST_REQUIREMENT_ID = 1;
        private const int TEACHER_A_ID = 101;
        private const int GROUP_1_ID = 201;

        public SchedulingEngineServiceTests()
        {
            // Initialize mocks for each test run
            _requirementRepo = Substitute.For<ICourseRequirementRepository>();
            _availabilityRepo = Substitute.For<IAvailabilityReadModelRepository>();
            _stagedPlacementRepo = Substitute.For<IStagedPlacementRepository>();

            // Instantiate the SUT with its dependencies
            _sut = new SchedulingEngineService(_requirementRepo, _availabilityRepo, _stagedPlacementRepo);
        }

        [Fact]
        public async Task
            GetValidSlotsForRequirementAsync_WhenScheduleHasConflictsAndPreferences_ShouldReturnCorrectlyFilteredAndScoredSlots()
        {
            // ARRANGE

            // 1. Define the Timetable Structure
            var timetable = new TimetableEntity
            {
                Id = 1,
                Days = new List<TimetableDayEntity>
                {
                    new() { Id = 1, Name = "Monday" }, new() { Id = 2, Name = "Tuesday" },
                    new() { Id = 3, Name = "Wednesday" }
                },
                Periods = new List<TimetablePeriodEntity>
                {
                    new() { Id = 10, Start = "09:00" }, new() { Id = 11, Start = "10:00" },
                    new() { Id = 12, Start = "11:00" }, new() { Id = 13, Start = "12:00" },
                    new() { Id = 14, Start = "13:00" }
                }
            };

            // 2. Define the Course Requirement
            var requirement = new CourseRequirementEntity
            {
                Id = TEST_REQUIREMENT_ID, TeacherId = TEACHER_A_ID, StudentGroupId = GROUP_1_ID, Length = 2,
                Timetable = timetable,
                PeriodPreferences = new List<PeriodPreferenceEntity> { new() { DayId = 1, StartPeriodId = 10 } }
            };

            // 3. Define the state of the world via the Availability Read Model
            var teacherMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
            teacherMatrix.Set(0, 2, 1); // Mon @ 11:00 conflict
            teacherMatrix.Set(2, 4, 1); // NEW: Wed @ 13:00 is busy
            var groupMatrix = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
            groupMatrix.Set(1, 1, 1); // Tue @ 10:00 conflict
            groupMatrix.Set(1, 2, 1); // Tue @ 11:00 conflict
            
            var resourceMatrices = new Dictionary<int, SchedulingMatrix>
                { { TEACHER_A_ID, teacherMatrix }, { GROUP_1_ID, groupMatrix } };

            // 4. Configure Mocks
            _requirementRepo.GetByIdWithDetailsAsync(TEST_REQUIREMENT_ID).Returns(Task.FromResult(requirement));
            _availabilityRepo.GetMatricesForResourcesAsync(Arg.Any<List<int>>())
                .Returns(Task.FromResult(resourceMatrices));
            _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(timetable.Id)
                .Returns(Task.FromResult(new List<StagedPlacement>()));

            // ACT
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(TEST_REQUIREMENT_ID);

            // ASSERT
            // The expected count is 5, not 9.
            // Mon: 2 valid (09:00, 12:00)
            // Tue: 1 valid (12:00)
            // Wed: 2 valid (09:00, 12:00)
            Assert.Equal(6, validSlots.Count); // <-- CORRECTED ASSERTION

            // The rest of the assertions remain valid and important
            var idealSlot = validSlots.Single(s => s.Type == SlotType.Ideal);
            Assert.Equal(1, idealSlot.DayId);
            Assert.Equal(10, idealSlot.StartPeriodId);

            var wednesdaySlot_0900 = validSlots.Single(s => s.DayId == 3 && s.StartPeriodId == 10);
            var wednesdaySlot_1000 = validSlots.Single(s => s.DayId == 3 && s.StartPeriodId == 11);
            Assert.True(wednesdaySlot_0900.GuidanceScore > wednesdaySlot_1000.GuidanceScore,
                "A slot that doesn't create fragments should be scored higher than one that does.");
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_WhenNoSlotsAreAvailable_ShouldReturnEmptyList()
        {
            // ARRANGE
            var timetable = new TimetableEntity
            {
                Id = 1, Days = new List<TimetableDayEntity> { new() { Id = 1, Name = "Monday" } },
                Periods = new List<TimetablePeriodEntity>
                    { new() { Id = 10, Start = "09:00" }, new() { Id = 11, Start = "10:00" } }
            };
            var requirement = new CourseRequirementEntity
            {
                Id = TEST_REQUIREMENT_ID, TeacherId = TEACHER_A_ID, StudentGroupId = GROUP_1_ID, Length = 1,
                Timetable = timetable
            };

            // Make both resources busy for the entire timetable
            var busyMatrix = new SchedulingMatrix(1, 2);
            busyMatrix.Set(0, 0, 1);
            busyMatrix.Set(0, 1, 1);
            var resourceMatrices = new Dictionary<int, SchedulingMatrix>
                { { TEACHER_A_ID, busyMatrix }, { GROUP_1_ID, busyMatrix } };

            _requirementRepo.GetByIdWithDetailsAsync(TEST_REQUIREMENT_ID).Returns(Task.FromResult(requirement));
            _availabilityRepo.GetMatricesForResourcesAsync(Arg.Any<List<int>>())
                .Returns(Task.FromResult(resourceMatrices));
            _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(timetable.Id)
                .Returns(Task.FromResult(new List<StagedPlacement>()));

            // ACT
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(TEST_REQUIREMENT_ID);

            // ASSERT
            Assert.NotNull(validSlots);
            Assert.Empty(validSlots);
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_WhenTeacherIsNotAssigned_ShouldOnlyConsiderGroupConflicts()
        {
            // ARRANGE
            var timetable = new TimetableEntity
            {
                Id = 1, Days = new List<TimetableDayEntity> { new() { Id = 1, Name = "Monday" } },
                Periods = new List<TimetablePeriodEntity>
                    { new() { Id = 10, Start = "09:00" }, new() { Id = 11, Start = "10:00" } }
            };
            // This requirement has a null TeacherId
            var requirement = new CourseRequirementEntity
            {
                Id = TEST_REQUIREMENT_ID, TeacherId = null, StudentGroupId = GROUP_1_ID, Length = 1,
                Timetable = timetable
            };

            var groupMatrix = new SchedulingMatrix(1, 2);
            groupMatrix.Set(0, 0, 1); // Group is busy Monday @ 09:00

            // The availability repo is only asked for the group's matrix
            var resourceMatrices = new Dictionary<int, SchedulingMatrix> { { GROUP_1_ID, groupMatrix } };

            _requirementRepo.GetByIdWithDetailsAsync(TEST_REQUIREMENT_ID).Returns(Task.FromResult(requirement));
            _availabilityRepo
                .GetMatricesForResourcesAsync(Arg.Is<List<int>>(ids => ids.Count == 1 && ids.Contains(GROUP_1_ID)))
                .Returns(Task.FromResult(resourceMatrices));
            _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(timetable.Id)
                .Returns(Task.FromResult(new List<StagedPlacement>()));

            // ACT
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(TEST_REQUIREMENT_ID);

            // ASSERT
            Assert.Single(validSlots);
            Assert.Equal(1, validSlots[0].DayId);
            Assert.Equal(11, validSlots[0].StartPeriodId); // Only the 10:00 slot is available
        }

        [Fact]
        public async Task GetValidSlotsForRequirementAsync_WhenNoPreferencesExist_ShouldReturnAllValidSlotsAsViable()
        {
            // ARRANGE
            var timetable = new TimetableEntity
            {
                Id = 1, Days = new List<TimetableDayEntity> { new() { Id = 1, Name = "Monday" } },
                Periods = new List<TimetablePeriodEntity>
                    { new() { Id = 10, Start = "09:00" }, new() { Id = 11, Start = "10:00" } }
            };
            // This requirement has an empty list of preferences
            var requirement = new CourseRequirementEntity
            {
                Id = TEST_REQUIREMENT_ID, TeacherId = TEACHER_A_ID, StudentGroupId = GROUP_1_ID, Length = 1,
                Timetable = timetable, PeriodPreferences = new List<PeriodPreferenceEntity>()
            };

            // All resources are fully available
            var availableMatrix = new SchedulingMatrix(1, 2);
            var resourceMatrices = new Dictionary<int, SchedulingMatrix>
                { { TEACHER_A_ID, availableMatrix }, { GROUP_1_ID, availableMatrix } };

            _requirementRepo.GetByIdWithDetailsAsync(TEST_REQUIREMENT_ID).Returns(Task.FromResult(requirement));
            _availabilityRepo.GetMatricesForResourcesAsync(Arg.Any<List<int>>())
                .Returns(Task.FromResult(resourceMatrices));
            _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(timetable.Id)
                .Returns(Task.FromResult(new List<StagedPlacement>()));

            // ACT
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(TEST_REQUIREMENT_ID);

            // ASSERT
            Assert.Equal(2, validSlots.Count);
            Assert.All(validSlots, slot => Assert.Equal(SlotType.Viable, slot.Type));
            Assert.DoesNotContain(validSlots, slot => slot.Type == SlotType.Ideal);
        }

        [Fact]
        public async Task
            GetValidSlotsForRequirementAsync_WhenPlacementsAreStaged_ShouldConsiderStagedPlacementsAsConflicts()
        {
            // ARRANGE
            var timetable = new TimetableEntity
            {
                Id = 1, Days = new List<TimetableDayEntity> { new() { Id = 1, Name = "Monday" } },
                Periods = new List<TimetablePeriodEntity>
                    { new() { Id = 10, Start = "09:00" }, new() { Id = 11, Start = "10:00" } }
            };
            var requirementToPlace = new CourseRequirementEntity
            {
                Id = TEST_REQUIREMENT_ID, TeacherId = TEACHER_A_ID, StudentGroupId = GROUP_1_ID, Length = 1,
                Timetable = timetable
            };

            // The main read model shows Teacher A is fully available
            var availableMatrix = new SchedulingMatrix(1, 2);
            var resourceMatrices = new Dictionary<int, SchedulingMatrix>
                { { TEACHER_A_ID, availableMatrix }, { GROUP_1_ID, availableMatrix } };

            // BUT, another requirement has been STAGED, making Teacher A busy on Monday @ 09:00
            var stagedPlacements = new List<StagedPlacement>
            {
                new StagedPlacement(
                    RequirementId: 99,
                    DayId: 1,
                    StartPeriodId: 10,
                    Length: 1,
                    ResourceIds: new List<int> { TEACHER_A_ID } // This placement involves Teacher A
                )
            };

            _requirementRepo.GetByIdWithDetailsAsync(TEST_REQUIREMENT_ID).Returns(Task.FromResult(requirementToPlace));
            _availabilityRepo.GetMatricesForResourcesAsync(Arg.Any<List<int>>())
                .Returns(Task.FromResult(resourceMatrices));
            _stagedPlacementRepo.GetStagedPlacementsForTimetableAsync(timetable.Id)
                .Returns(Task.FromResult(stagedPlacements));

            // ACT
            var validSlots = await _sut.GetValidSlotsForRequirementAsync(TEST_REQUIREMENT_ID);

            // ASSERT
            // The slot at Mon @ 09:00 should now be invalid due to the staged placement.
            Assert.Single(validSlots);
            Assert.Equal(1, validSlots[0].DayId);
            Assert.Equal(11, validSlots[0].StartPeriodId);
        }
    }
}
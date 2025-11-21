using Api.Controllers;
using Api.Data;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kawsay.UnitTests.Api.Controllers;

public class TimetableControllerTests
{
    [Fact]
    public async Task GetTimetables_ReturnsFullStructures()
    {
        var controller = CreateController(new[]
        {
            BuildTimetableEntity(1, "Draft 1", new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7)),
            BuildTimetableEntity(2, "Draft 2", new DateOnly(2024, 2, 1), new DateOnly(2024, 2, 7)),
        });

        var response = await controller.GetTimetables();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var structures = Assert.IsAssignableFrom<IEnumerable<TimetableStructure>>(okResult.Value).ToList();
        Assert.Equal(2, structures.Count);
        Assert.All(structures, structure =>
        {
            Assert.NotEmpty(structure.Days);
            Assert.NotEmpty(structure.Periods);
        });
    }

    [Fact]
    public async Task GetMasterTimetable_NoTimetables_ReturnsNotFound()
    {
        var controller = CreateController();

        var response = await controller.GetMasterTimetable();

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetMasterTimetable_ReturnsLatestByEndDate()
    {
        var timetables = new[]
        {
            BuildTimetableEntity(1, "Draft 1", new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7)),
            BuildTimetableEntity(2, "Draft 2", new DateOnly(2024, 3, 1), new DateOnly(2024, 3, 7)),
        };
        var controller = CreateController(timetables);

        var response = await controller.GetMasterTimetable();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var master = Assert.IsType<TimetableStructure>(okResult.Value);
        Assert.Equal(2, master.Id);
    }

    [Fact]
    public async Task PublishTimetable_MissingId_ReturnsNotFound()
    {
        var controller = CreateController();

        var response = await controller.PublishTimetable(99);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PublishTimetable_WhenFound_ReturnsOkWithStructure()
    {
        var controller = CreateController(new[]
        {
            BuildTimetableEntity(7, "Draft", new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7)),
        });

        var response = await controller.PublishTimetable(7);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var structure = Assert.IsType<TimetableStructure>(okResult.Value);
        Assert.Equal(7, structure.Id);
    }

    [Fact]
    public async Task GetCohortsForTimetable_ReturnsMappedDtos()
    {
        var cohorts = new[]
        {
            new CohortEntity
            {
                Id = 10,
                Name = "Cohort A",
                TimetableId = 1,
                StudentGroups = new List<StudentGroupEntity>
                {
                    new() { Id = 100, Name = "Group 1", Sections = new List<SectionEntity>() }
                }
            }
        };
        var controller = CreateController(
            timetableSeed: new[] { BuildTimetableEntity(1, "Draft", new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7)) },
            cohortSeed: cohorts);

        var response = await controller.GetCohortsForTimetable(1);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<CohortDetailDto>>(okResult.Value).ToList();
        Assert.Single(payload);
        Assert.Equal("Cohort A", payload[0].Name);
        Assert.Single(payload[0].StudentGroups);
    }

    private static TimetableController CreateController(
        IEnumerable<TimetableEntity>? timetableSeed = null,
        IEnumerable<CohortEntity>? cohortSeed = null)
    {
        var timetableRepository = new InMemoryTimetableRepository(timetableSeed ?? Enumerable.Empty<TimetableEntity>());
        var timetableService = new TimetableService(timetableRepository);
        var academicStructureService = new AcademicStructureService(
            new StubAcademicStructureRepository(cohortSeed ?? Enumerable.Empty<CohortEntity>()),
            timetableRepository,
            new NullStudentRepository());
        return new TimetableController(timetableService, academicStructureService);
    }

    private static TimetableEntity BuildTimetableEntity(int id, string name, DateOnly start, DateOnly end)
    {
        return new TimetableEntity
        {
            Id = id,
            Name = name,
            StartDate = start,
            EndDate = end,
            Days = new List<TimetableDayEntity>
            {
                new() { Id = id * 10 + 1, Name = "Monday" }
            },
            Periods = new List<TimetablePeriodEntity>
            {
                new() { Id = id * 100 + 1, Start = "08:00", End = "09:00" }
            }
        };
    }

    private sealed class InMemoryTimetableRepository : ITimetableRepository
    {
        private readonly List<TimetableEntity> _storage;

        public InMemoryTimetableRepository(IEnumerable<TimetableEntity> seed)
        {
            _storage = seed.Select(Clone).ToList();
        }

        public Task<TimetableEntity?> GetByIdAsync(int id) =>
            Task.FromResult(_storage.FirstOrDefault(t => t.Id == id));

        public Task<IEnumerable<TimetableEntity>> GetAllAsync() =>
            Task.FromResult<IEnumerable<TimetableEntity>>(_storage.Select(Clone).ToList());

        public Task<TimetableEntity> AddAsync(TimetableEntity timetable)
        {
            _storage.Add(Clone(timetable));
            return Task.FromResult(Clone(timetable));
        }

        private static TimetableEntity Clone(TimetableEntity source)
        {
            return new TimetableEntity
            {
                Id = source.Id,
                Name = source.Name,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Days = source.Days.Select(d => new TimetableDayEntity { Id = d.Id, Name = d.Name }).ToList(),
                Periods = source.Periods.Select(p => new TimetablePeriodEntity { Id = p.Id, Start = p.Start, End = p.End }).ToList()
            };
        }
    }

    private sealed class StubAcademicStructureRepository : IAcademicStructureRepository
    {
        private readonly Dictionary<int, List<CohortEntity>> _cohortsByTimetable;

        public StubAcademicStructureRepository(IEnumerable<CohortEntity> cohorts)
        {
            _cohortsByTimetable = cohorts
                .GroupBy(c => c.TimetableId)
                .ToDictionary(g => g.Key, g => g.Select(CloneCohort).ToList());
        }

        public Task<CohortEntity> AddCohortAsync(CohortEntity cohort) => throw new NotImplementedException();
        public Task<CohortEntity?> GetCohortByIdAsync(int cohortId) => throw new NotImplementedException();

        public Task<List<CohortEntity>> GetCohortsByTimetableAsync(int timetableId)
        {
            return Task.FromResult(_cohortsByTimetable.TryGetValue(timetableId, out var cohorts)
                ? cohorts.Select(CloneCohort).ToList()
                : new List<CohortEntity>());
        }

        public Task<StudentGroupEntity> AddStudentGroupAsync(StudentGroupEntity studentGroup) => throw new NotImplementedException();
        public Task<StudentGroupEntity?> GetStudentGroupByIdAsync(int groupId) => throw new NotImplementedException();
        public Task<SectionEntity> AddSectionAsync(SectionEntity section) => throw new NotImplementedException();
        public Task<SectionEntity?> GetSectionWithStudentsAsync(int sectionId) => throw new NotImplementedException();
        public Task AssignStudentToSectionAsync(int studentId, int sectionId) => throw new NotImplementedException();

        private static CohortEntity CloneCohort(CohortEntity source)
        {
            return new CohortEntity
            {
                Id = source.Id,
                Name = source.Name,
                TimetableId = source.TimetableId,
                StudentGroups = source.StudentGroups.Select(g => new StudentGroupEntity
                {
                    Id = g.Id,
                    Name = g.Name,
                    Sections = g.Sections.Select(s => new SectionEntity { Id = s.Id, Name = s.Name }).ToList()
                }).ToList()
            };
        }
    }

    private sealed class NullStudentRepository : IStudentRepository
    {
        public Task<StudentEntity?> GetByIdAsync(int id) => Task.FromResult<StudentEntity?>(null);
        public Task<List<StudentEntity>> GetByIdsAsync(IEnumerable<int> studentIds) => Task.FromResult(new List<StudentEntity>());
        public Task<IEnumerable<StudentEntity>> GetAllAsync() => Task.FromResult<IEnumerable<StudentEntity>>(Array.Empty<StudentEntity>());
        public Task<StudentEntity> AddAsync(StudentEntity student) => throw new NotImplementedException();
        public Task UpdateAsync(StudentEntity student) => throw new NotImplementedException();
        public Task UpdateRangeAsync(IEnumerable<StudentEntity> students) => throw new NotImplementedException();
    }
}


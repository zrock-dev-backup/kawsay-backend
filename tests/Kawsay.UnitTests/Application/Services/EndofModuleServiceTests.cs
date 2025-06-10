using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Services;
using Domain.Entities;
using NSubstitute;

namespace Kawsay.UnitTests.Application.Services;

public class EndofModuleServiceTests
{
    private readonly IStudentModuleGradeRepository _gradeRepository;
    private readonly ITimetableRepository _timetableRepository;
    private readonly EndofModuleService _sut;

    public EndofModuleServiceTests()
    {
        _gradeRepository = Substitute.For<IStudentModuleGradeRepository>();
        var studentRepository = Substitute.For<IStudentRepository>();
        _timetableRepository = Substitute.For<ITimetableRepository>();

        _sut = new EndofModuleService(_gradeRepository, studentRepository, _timetableRepository);
    }

    #region IngestGradesAsync Tests

    [Fact]
    public async Task IngestGradesAsync_WithValidData_CallsRepositoryWithCorrectlyMappedData()
    {
        // Arrange
        const int timetableId = 1;
        var timetable = new TimetableEntity() { Id = timetableId };
        _timetableRepository.GetByIdAsync(timetableId).Returns(timetable);

        var gradeData = new List<GradeIngestionDto>
        {
            new() { StudentId = 1, CourseId = 101, GradeValue = 85.0m }, // Pass
            new() { StudentId = 2, CourseId = 101, GradeValue = 65.0m }  // Fail
        };

        // Act
        await _sut.IngestGradesAsync(timetableId, gradeData);

        // Assert
        // Verify that AddRangeAsync was called once with a list of 2 items
        await _gradeRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<StudentModuleGrade>>(
            grades => grades.Count() == 2 &&
                      grades.First().StudentId == 1 &&
                      grades.First().IsPassing == true &&
                      grades.Last().StudentId == 2 &&
                      grades.Last().IsPassing == false
        ));
    }

    [Fact]
    public async Task IngestGradesAsync_WithInvalidTimetableId_ThrowsArgumentException()
    {
        // Arrange
        const int invalidTimetableId = 99;
        _timetableRepository.GetByIdAsync(invalidTimetableId).Returns((TimetableEntity?)null);
        var gradeData = new List<GradeIngestionDto> { new() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.IngestGradesAsync(invalidTimetableId, gradeData)
        );

        Assert.Equal($"Timetable with ID {invalidTimetableId} not found.", exception.Message);
        await _gradeRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<StudentModuleGrade>>());
    }

    #endregion

    #region SegmentCohortsAsync Tests

    [Fact]
    public async Task SegmentCohortsAsync_WithMixedGrades_CorrectlySegmentsStudents()
    {
        // Arrange
        const int timetableId = 1;
        var grades = new List<StudentModuleGrade>
        {
            // Student 1 passes both
            new() { StudentId = 1, CourseId = 101, IsPassing = true },
            new() { StudentId = 1, CourseId = 102, IsPassing = true },
            // Student 2 fails one
            new() { StudentId = 2, CourseId = 101, IsPassing = true },
            new() { StudentId = 2, CourseId = 102, IsPassing = false },
            // Student 3 passes all
            new() { StudentId = 3, CourseId = 101, IsPassing = true },
        };
        _gradeRepository.GetGradesByTimetableIdAsync(timetableId).Returns(grades);
        
        // TODO: Service is using dummy DTOs, IStudentRepository should be mocked.

        // Act
        var result = await _sut.SegmentCohortsAsync(timetableId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.AdvancingStudents.Count);
        Assert.Single(result.RetakeStudents);
        Assert.Contains(result.AdvancingStudents, s => s.Id == 1);
        Assert.Contains(result.AdvancingStudents, s => s.Id == 3);
        Assert.Contains(result.RetakeStudents, s => s.Id == 2);
    }

    [Fact]
    public async Task SegmentCohortsAsync_WhenAllStudentsPass_ReturnsAllInAdvancingCohort()
    {
        // Arrange
        const int timetableId = 1;
        var grades = new List<StudentModuleGrade>
        {
            new() { StudentId = 1, IsPassing = true },
            new() { StudentId = 2, IsPassing = true },
        };
        _gradeRepository.GetGradesByTimetableIdAsync(timetableId).Returns(grades);

        // Act
        var result = await _sut.SegmentCohortsAsync(timetableId);

        // Assert
        Assert.Equal(2, result.AdvancingStudents.Count);
        Assert.Empty(result.RetakeStudents);
    }

    [Fact]
    public async Task SegmentCohortsAsync_WhenAllStudentsFail_ReturnsAllInRetakeCohort()
    {
        // Arrange
        const int timetableId = 1;
        var grades = new List<StudentModuleGrade>
        {
            new() { StudentId = 1, IsPassing = false },
            new() { StudentId = 2, IsPassing = false },
        };
        _gradeRepository.GetGradesByTimetableIdAsync(timetableId).Returns(grades);

        // Act
        var result = await _sut.SegmentCohortsAsync(timetableId);

        // Assert
        Assert.Empty(result.AdvancingStudents);
        Assert.Equal(2, result.RetakeStudents.Count);
    }

    [Fact]
    public async Task SegmentCohortsAsync_WithNoGradesForModule_ReturnsEmptyCohorts()
    {
        // Arrange
        const int timetableId = 1;
        _gradeRepository.GetGradesByTimetableIdAsync(timetableId).Returns(new List<StudentModuleGrade>());

        // Act
        var result = await _sut.SegmentCohortsAsync(timetableId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.AdvancingStudents);
        Assert.Empty(result.RetakeStudents);
    }
    
    #endregion
}

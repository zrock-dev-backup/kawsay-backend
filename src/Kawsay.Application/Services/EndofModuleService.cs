using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EndofModuleService(
    IStudentModuleGradeRepository gradeRepository,
    IStudentRepository studentRepository,
    ITimetableRepository timetableRepository)
{
    private const decimal PassingThreshold = 70.0m; // Example passing grade

    public async Task IngestGradesAsync(int timetableId, IEnumerable<GradeIngestionDto> gradeData)
    {
        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null)
        {
            throw new ArgumentException($"Timetable with ID {timetableId} not found.");
        }

        var newGrades = gradeData.Select(g => new StudentModuleGrade
        {
            StudentId = g.StudentId,
            CourseId = g.CourseId,
            TimetableId = timetableId,
            GradeValue = g.GradeValue,
            IsPassing = g.GradeValue >= PassingThreshold
        }).ToList();

        await gradeRepository.AddRangeAsync(newGrades);
    }

    public async Task<StudentCohortDto> SegmentCohortsAsync(int timetableId)
    {
        var grades = await gradeRepository.GetGradesByTimetableIdAsync(timetableId);
        var studentIdsWithFailingGrades = grades
            .Where(g => !g.IsPassing)
            .Select(g => g.StudentId)
            .Distinct()
            .ToHashSet();

        var allStudentIdsInModule = grades.Select(g => g.StudentId).Distinct().ToList();
        
        // Dummy DTOs.
        var advancingStudents = new List<StudentDto>();
        var retakeStudents = new List<StudentDto>();

        foreach (var studentId in allStudentIdsInModule)
        {
            var studentDto = new StudentDto { Id = studentId, Name = $"Student {studentId}" }; // Placeholder
            if (studentIdsWithFailingGrades.Contains(studentId))
            {
                retakeStudents.Add(studentDto);
            }
            else
            {
                advancingStudents.Add(studentDto);
            }
        }

        return new StudentCohortDto
        {
            AdvancingStudents = advancingStudents,
            RetakeStudents = retakeStudents
        };
    }

    public async Task<BulkActionResponse> BulkAdvanceStudentsAsync(BulkAdvanceRequest request)
    {
        if (request.StudentIds == null || !request.StudentIds.Any())
        {
            throw new ArgumentException("At least one student ID must be provided.", nameof(request.StudentIds));
        }

        var students = await studentRepository.GetByIdsAsync(request.StudentIds);

        if (students.Count != request.StudentIds.Count)
        {
            var foundIds = students.Select(s => s.Id).ToList();
            var missingIds = request.StudentIds.Except(foundIds);
            throw new ArgumentException($"Could not find all students. Missing IDs: {string.Join(", ", missingIds)}");
        }

        foreach (var student in students)
        {
            student.Standing = AcademicStanding.GoodStanding;
        }

        await studentRepository.UpdateRangeAsync(students);

        return new BulkActionResponse(
            $"{students.Count} student(s) successfully advanced to Good Standing.",
            students.Count
        );
    }
}

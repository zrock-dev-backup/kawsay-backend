using Application.DTOs;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

// ... other usings

namespace Application.Services;

public class EndofModuleService(
    IStudentModuleGradeRepository gradeRepository,
    IStudentRepository studentRepository,
    ITimetableRepository timetableRepository,
    IAcademicApiClient academicApiClient) // <-- INJECTED
{
    private const decimal PassingThreshold = 70.0m;

    // This method replaces the old manual IngestGradesAsync
    public async Task SyncGradesFromSisAsync(int timetableId, List<int> localStudentIds)
    {
        var timetable = await timetableRepository.GetByIdAsync(timetableId) 
            ?? throw new ArgumentException($"Timetable {timetableId} not found.");

        var students = await studentRepository.GetByIdsAsync(localStudentIds);
        var newGrades = new List<StudentModuleGrade>();

        foreach (var student in students)
        {
            // IMPORTANT: Requires StudentEntity to have an ExternalId to query the SIS
            var sisId = "STU-10485"; // STUB: student.ExternalId; 

            var sisProfile = await academicApiClient.GetStudentProfileAsync(sisId);
            
            if (sisProfile?.Grades != null)
            {
                foreach (var externalGrade in sisProfile.Grades)
                {
                    // Map SIS grade to local read-model
                    if (decimal.TryParse(externalGrade.GradeValue, out var numericGrade))
                    {
                        newGrades.Add(new StudentModuleGrade
                        {
                            StudentId = student.Id,
                            CourseId = 1, // STUB: Resolve from externalGrade.CourseCode
                            TimetableId = timetableId,
                            GradeValue = numericGrade,
                            IsPassing = numericGrade >= PassingThreshold
                        });
                    }
                }
            }
        }

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

using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Services;

public class EnrollmentService(
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IEnrollmentRepository enrollmentRepository,
    ICourseRepository courseRepository)
{
    public async Task<EnrollmentEntity> EnrollStudentAsync(EnrollmentRequestDto request)
    {
        var student = await studentRepository.GetByIdAsync(request.StudentId) ??
                      throw new ValidationException($"Student with ID {request.StudentId} not found.");

        var classToEnroll = await classRepository.GetByIdAsync(request.ClassId) ??
                            throw new ValidationException($"Class with ID {request.ClassId} not found.");

        if (!request.Force)
        {
            await ValidatePrerequisites(student, classToEnroll.CourseId);
            await ValidateCapacity(classToEnroll);
            await ValidateTimeClash(student, classToEnroll);
        }

        var enrollment = new EnrollmentEntity
        {
            StudentId = student.Id,
            ClassId = classToEnroll.Id,
            EnrollmentDate = DateTime.UtcNow
        };

        return await enrollmentRepository.AddAsync(enrollment);
    }

    private async Task ValidatePrerequisites(StudentEntity student, int courseId)
    {
        // TODO: Enhance prerequisite validation logic.
        Console.WriteLine($"Prerequisite validation for Student {student.Id} and Course {courseId}.");
        await Task.CompletedTask;
    }

    private static Task ValidateCapacity(ClassEntity classToEnroll)
    {
        if (classToEnroll.Enrollments.Count >= classToEnroll.Capacity)
        {
            throw new ValidationException(
                $"Class '{classToEnroll.Course.Name}' is full. Capacity is {classToEnroll.Capacity}.");
        }

        return Task.CompletedTask;
    }

    private async Task ValidateTimeClash(StudentEntity student, ClassEntity classToEnroll)
    {
        var studentEnrollments =
            await enrollmentRepository.GetEnrollmentsForStudentAsync(student.Id, classToEnroll.TimetableId);

        var studentSchedule = studentEnrollments
            .SelectMany(e => e.Class.ClassOccurrences)
            .ToHashSet(new ClassOccurrenceComparer());

        var newClassSchedule = classToEnroll.ClassOccurrences
            .ToHashSet(new ClassOccurrenceComparer());

        if (newClassSchedule.Overlaps(studentSchedule))
        {
            throw new ValidationException("Enrollment failed due to a time clash with an existing class.");
        }
    }

    // TODO: implement overlapped classes scenario
    private class ClassOccurrenceComparer : IEqualityComparer<ClassOccurrenceEntity>
    {
        public bool Equals(ClassOccurrenceEntity? x, ClassOccurrenceEntity? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Date == y.Date && x.StartPeriodId == y.StartPeriodId;
        }

        public int GetHashCode(ClassOccurrenceEntity obj)
        {
            return HashCode.Combine(obj.Date, obj.StartPeriodId);
        }
    }
}
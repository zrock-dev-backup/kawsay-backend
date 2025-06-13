using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class EnrollmentService(
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IEnrollmentRepository enrollmentRepository,
    ITimetableRepository timetableRepository)
{
    public async Task<EnrollmentEntity> EnrollStudentAsync(EnrollmentRequestDto request)
    {
        var student = await studentRepository.GetByIdAsync(request.StudentId) ??
                      throw new ValidationException($"Student with ID {request.StudentId} not found.");

        var classToEnroll = await classRepository.GetByIdAsync(request.ClassId) ??
                            throw new ValidationException($"Class with ID {request.ClassId} not found.");

        if (!request.Force)
        {
            await ValidateTimeClash(student, classToEnroll);
            await ValidateCapacity(classToEnroll);
            await ValidatePrerequisites(student, classToEnroll.CourseId);
        }

        var enrollment = new EnrollmentEntity
        {
            StudentId = student.Id,
            ClassId = classToEnroll.Id,
            EnrollmentDate = DateTime.UtcNow
        };

        return await enrollmentRepository.AddAsync(enrollment);
    }

    private async Task ValidateTimeClash(StudentEntity student, ClassEntity classToEnroll)
    {
        var timetable = await timetableRepository.GetByIdAsync(classToEnroll.TimetableId) ??
                        throw new InvalidOperationException("Could not load timetable for validation.");

        var studentEnrollments =
            await enrollmentRepository.GetEnrollmentsForStudentAsync(student.Id, classToEnroll.TimetableId);


        var scheduledItems = studentEnrollments.Select(e => e.Class).ToList();


        var availabilityMatrix = new ResourceAvailabilityMatrix(timetable, scheduledItems);

        if (availabilityMatrix.HasClash(classToEnroll))
        {
            throw new ValidationException("Enrollment failed due to a time clash with an existing class.");
        }
    }

    private async Task ValidateCapacity(ClassEntity classToEnroll)
    {
        var currentEnrollmentCount = await enrollmentRepository.CountByClassIdAsync(classToEnroll.Id);
        if (currentEnrollmentCount >= classToEnroll.Capacity)
        {
            throw new ValidationException(
                $"Class '{classToEnroll.Course.Name}' is full. Capacity is {classToEnroll.Capacity}.");
        }
    }

    private Task ValidatePrerequisites(StudentEntity student, int courseId)
    {
        return Task.CompletedTask;
    }
}
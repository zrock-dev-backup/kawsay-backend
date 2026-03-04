using Application.Core;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class TimetableAssignmentService(
    ITimetableAssignmentRepository assignmentRepository,
    ITimetableRepository timetableRepository,
    ITeacherRepository teacherRepository
    ) : ITimetableAssignmentService
{
    public async Task<Result<IEnumerable<TimetableAssignmentDto>>> GetAssignmentsAsync(int timetableId)
    {
        var assignments = await assignmentRepository.GetByTimetableIdAsync(timetableId);
        var dtos = assignments.Select(MapToDto);
        return Result<IEnumerable<TimetableAssignmentDto>>.Success(dtos);
    }

    public async Task<Result<TimetableAssignmentDto>> CreateAssignmentAsync(int timetableId, CreateTimetableAssignmentRequestDto request)
    {
        // Validate Timetable
        var timetable = await timetableRepository.GetByIdAsync(timetableId);
        if (timetable == null)
            return Result<TimetableAssignmentDto>.Failure(Error.NotFound("Timetable.NotFound", "Timetable not found"));

        // Validate Teacher
        var teacher = await teacherRepository.GetByIdAsync(request.TeacherId);
        if (teacher == null)
            return Result<TimetableAssignmentDto>.Failure(Error.NotFound("Teacher.NotFound", "Teacher not found"));

        // Parse Enum manually to handle "Hours per Week" space
        var workloadUnit = ParseWorkloadUnit(request.WorkloadUnit);

        var entity = new TimetableAssignmentEntity
        {
            TimetableId = timetableId,
            TeacherId = request.TeacherId,
            StartWeek = request.StartWeek,
            EndWeek = request.EndWeek,
            MaximumWorkload = request.MaximumWorkload,
            WorkloadUnit = workloadUnit
        };

        await assignmentRepository.AddAsync(entity);
        await assignmentRepository.SaveChangesAsync();

        // Reload to get relationships if needed, or map manually since we have the Teacher object
        entity.Teacher = teacher; 
        return Result<TimetableAssignmentDto>.Success(MapToDto(entity));
    }

    public async Task<Result> DeleteAssignmentAsync(int assignmentId)
    {
        var entity = await assignmentRepository.GetByIdAsync(assignmentId);
        if (entity == null)
            return Result.Failure(Error.NotFound("Assignment.NotFound", "Assignment not found"));

        await assignmentRepository.DeleteAsync(entity);
        await assignmentRepository.SaveChangesAsync();
        return Result.Success();
    }

    private static TimetableAssignmentDto MapToDto(TimetableAssignmentEntity entity)
    {
        // Map Enum back to Frontend String
        string unitString = entity.WorkloadUnit == WorkloadUnit.HoursPerWeek ? "Hours per Week" : "Classes";

        return new TimetableAssignmentDto(
            entity.Id,
            entity.TimetableId,
            entity.TeacherId,
            entity.StartWeek,
            entity.EndWeek,
            entity.MaximumWorkload,
            unitString
        );
    }

    private static WorkloadUnit ParseWorkloadUnit(string unit)
    {
        return unit.Trim().ToLower() switch
        {
            "classes" => WorkloadUnit.Classes,
            "hours per week" => WorkloadUnit.HoursPerWeek,
            "hoursperweek" => WorkloadUnit.HoursPerWeek,
            _ => WorkloadUnit.Classes // Default fallback
        };
    }
}

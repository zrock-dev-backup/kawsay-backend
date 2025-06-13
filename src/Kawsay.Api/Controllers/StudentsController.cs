using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("kawsay/[controller]")]
public class StudentsController(
    IStudentRepository studentRepository,
    IClassRepository classRepository,
    IEnrollmentRepository enrollmentRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents([FromQuery] int? timetableId)
    {
        var students = await studentRepository.GetAllAsync();
        var dtos = new List<StudentDto>();

        foreach (var s in students)
        {
            var courseLoad = 0;
            if (timetableId.HasValue)
            {
                var enrollments = await enrollmentRepository.GetEnrollmentsForStudentAsync(s.Id, timetableId.Value);
                courseLoad = enrollments.Count;
            }

            dtos.Add(new StudentDto
            {
                Id = s.Id,
                Name = s.Name,
                Standing = s.Standing.ToString(),
                CurrentCourseLoad = courseLoad
            });
        }

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] StudentDto studentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var studentEntity = new StudentEntity
        {
            Name = studentDto.Name,
            Standing = Enum.TryParse<AcademicStanding>(studentDto.Standing, true, out var standing)
                ? standing
                : AcademicStanding.GoodStanding
        };

        var createdStudent = await studentRepository.AddAsync(studentEntity);

        var responseDto = new StudentDto
        {
            Id = createdStudent.Id,
            Name = createdStudent.Name,
            Standing = createdStudent.Standing.ToString()
        };

        return Created($"/kawsay/Students/{responseDto.Id}", responseDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDto>> GetStudent(int id)
    {
        var student = await studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        var dto = new StudentDto
        {
            Id = student.Id,
            Name = student.Name,
            Standing = student.Standing.ToString()
        };
        return Ok(dto);
    }

    [HttpGet("{studentId:int}/available-classes")]
    public async Task<ActionResult<IEnumerable<AvailableClassDto>>> GetAvailableClassesForStudent(int studentId,
        [FromQuery] int timetableId)
    {
        var student = await studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            return NotFound(new { message = $"Student with ID {studentId} not found." });
        }

        var allTimetableClasses = await classRepository.GetAllAsync(timetableId);
        var response = allTimetableClasses.Select(cls => new AvailableClassDto
            {
                Id = cls.Id,
                StartDate = cls.StartDate,
                EndDate = cls.EndDate,
                TimetableId = cls.TimetableId,
                CourseId = cls.Course.Id,
                CourseName = cls.Course.Name,
                CourseCode = cls.Course.Code,
                TeacherId = cls.Teacher?.Id,
                TeacherName = cls.Teacher?.Name,
                Length = cls.Length,
                Frequency = cls.Frequency,
                ClassType = cls.ClassType == ClassType.Masterclass ? ClassTypeDto.Masterclass : ClassTypeDto.Lab,
                Capacity = cls.Capacity,
                CurrentEnrollment = cls.Enrollments.Count,
                ClassOccurrences = cls.ClassOccurrences.Select(o => new ClassOccurrenceDto
                    { Date = o.Date, StartPeriodId = o.StartPeriodId }).ToList(),
                PeriodPreferences = cls.PeriodPreferences.Select(p => new DayPeriodPreferenceDto
                    { DayId = p.DayId, StartPeriodId = p.StartPeriodId }).ToList(),
                IsEligible = true // Assume eligible by default
            })
            .ToList();

        // TODO: Add sorting logic to put retakes first.
        return Ok(response);
    }
}
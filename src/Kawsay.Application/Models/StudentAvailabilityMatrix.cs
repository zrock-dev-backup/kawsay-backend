using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.Models;

public class StudentAvailabilityMatrix
{
    private readonly int[,] _matrix;
    private readonly Dictionary<DayOfWeek, int> _dayMap;
    private readonly Dictionary<int, int> _periodMap;
    private readonly int _periodCount;

    private StudentAvailabilityMatrix(TimetableEntity timetable, List<EnrollmentEntity> enrollments)
    {
        var sortedDays = timetable.Days
            .OrderBy(d => (int)(Enum.Parse<DayOfWeek>(d.Name, true) + 6) % 7) // Sort Mon-Sun
            .ToList();
        
        var sortedPeriods = timetable.Periods.OrderBy(p => p.Start).ToList();

        _dayMap = sortedDays.Select((day, index) => new { DayOfWeek = Enum.Parse<DayOfWeek>(day.Name, true), Index = index })
            .ToDictionary(x => x.DayOfWeek, x => x.Index);
        
        _periodMap = sortedPeriods.Select((period, index) => new { period.Id, Index = index })
            .ToDictionary(x => x.Id, x => x.Index);

        _periodCount = sortedPeriods.Count;
        _matrix = new int[sortedDays.Count, _periodCount];
        
        PopulateMatrix(enrollments);
    }

    private void PopulateMatrix(IEnumerable<EnrollmentEntity> enrollments)
    {
        foreach (var enrollment in enrollments)
        foreach (var occurrence in enrollment.Class.ClassOccurrences)
        {
            if (!_dayMap.TryGetValue(occurrence.Date.DayOfWeek, out var dayIndex)) continue;
            if (!_periodMap.TryGetValue(occurrence.StartPeriodId, out var startPeriodIndex)) continue;
            
            for (var i = 0; i < enrollment.Class.Length; i++)
            {
                var currentPeriodIndex = startPeriodIndex + i;
                if (currentPeriodIndex < _periodCount)
                {
                    _matrix[dayIndex, currentPeriodIndex] = 1; // Mark as busy
                }
            }
        }
    }

    public bool HasClash(ClassEntity classToCheck)
    {
        foreach (var occurrence in classToCheck.ClassOccurrences)
        {
            if (!_dayMap.TryGetValue(occurrence.Date.DayOfWeek, out var dayIndex)) continue;
            if (!_periodMap.TryGetValue(occurrence.StartPeriodId, out var startPeriodIndex))
            {
                throw new InvalidOperationException($"Period ID {occurrence.StartPeriodId} not found in timetable map during clash detection.");
            }

            for (var i = 0; i < classToCheck.Length; i++)
            {
                var currentPeriodIndex = startPeriodIndex + i;
                if (currentPeriodIndex >= _periodCount) continue;
                
                if (_matrix[dayIndex, currentPeriodIndex] == 1)
                {
                    return true; // Clash detected
                }
            }
        }
        return false; // No clashes found
    }

    public static async Task<StudentAvailabilityMatrix> CreateForStudentAsync(
        IEnrollmentRepository enrollmentRepo, 
        ITimetableRepository timetableRepo, 
        int studentId, 
        int timetableId)
    {
        var timetable = await timetableRepo.GetByIdAsync(timetableId) ??
                       throw new InvalidOperationException("Could not load timetable for availability matrix generation.");
        var enrollments = await enrollmentRepo.GetEnrollmentsForStudentAsync(studentId, timetableId);
        
        return new StudentAvailabilityMatrix(timetable, enrollments);
    }
}

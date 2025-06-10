using Application.Features.Scheduling.Algorithm;
using Application.Features.Scheduling.Models;

namespace Kawsay.UnitTests.Scheduling;

public class SchedulingAlgorithmTest
{
    private const int NumDays = 5;
    private const int NumPeriods = 26;
        
    private static SchedulingRequirementLine MakeMasterClass(List<int> ids)
    {
        var x = new SchedulingRequirementLine(2, 4, ids, NumDays, NumPeriods);
        const int dayId = 1;
        x.PeriodPreferenceMatrix.Set(dayId, 1, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 2, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 3, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 4, 0);
        
        x.PeriodPreferenceMatrix.Set(dayId, 6, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 7, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 8, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 9, 0);
        
        x.PeriodPreferenceMatrix.Set(dayId, 12, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 13, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 14, 0);
        x.PeriodPreferenceMatrix.Set(dayId, 15, 0);
        
        return x;
    }

    private static SchedulingRequirementLine MakeSection(List<int> ids, List<int> periods)
    {
        var x = new SchedulingRequirementLine(1, 1, ids, NumDays, NumPeriods);
        const int dayId = 1;
        foreach (var value in periods)
        {
            x.PeriodPreferenceMatrix.Set(dayId, value, 0);
        }
        return x;
    }
    
    private static void PrintSchedule(LinkedList<SchedulingRequirementLine> requirementLines, List<SchedulingEntity> entities)
    {
        foreach (var requirementLine in requirementLines)
        {
            var iEntities = entities.Where(entity => requirementLine.EntitiesList.Contains(entity.Id)).ToList();
            var name = "";
            foreach (var iEntity in iEntities)
            {
                name += $" {iEntity.Name}";
            }

            var periods = "";
            foreach (var keyValuePair in requirementLine.AssignedTimeslotList)
            {
                periods += $" [{keyValuePair.Day}, {keyValuePair.Period}]";
            }

            Console.WriteLine($"Entities: {name}, Periods: {periods}");
        }
    }

    [Fact]
    public void TimetableGeneration_AlgorithmShouldReturnPopulatedTimetable()
    {
        List<SchedulingEntity> entities = [
            new(0,"Luz Florez", NumDays, NumPeriods),
            new(1, "Lecture - Logic MasterClass", NumDays, NumPeriods),
            new(2, "Lecture - Logic Section-A", NumDays, NumPeriods),
            new(3, "Lecture - Logic Section-B", NumDays, NumPeriods),
            new(4, "Lecture - Logic Section-C", NumDays, NumPeriods),
            new(5, "Lecture - Logic Section-E", NumDays, NumPeriods),
            new(6, "Elayne Ferreira", NumDays, NumPeriods),
            new(7, "Lecture - Programming MasterClass", NumDays, NumPeriods),
            new(8, "Lecture - Programming Section-A", NumDays, NumPeriods),
            new(9, "Lecture - Programming Section-B", NumDays, NumPeriods),
            new(10, "Lecture - Programming Section-C", NumDays, NumPeriods),
            new(11, "Lecture - Programming Section-E", NumDays, NumPeriods),
        ];

        var document = new LinkedList<SchedulingRequirementLine>();
        document.AddFirst(MakeMasterClass([0, 7]));
        document.AddLast(MakeMasterClass([0, 1]));
        
        document.AddLast(MakeSection([0, 2], [1, 6, 12]));
        document.AddLast(MakeSection([0, 3], [2, 7, 13]));
        document.AddLast(MakeSection([0, 4], [3, 8, 14]));
        document.AddLast(MakeSection([0, 5], [4, 9, 15]));
        
        document.AddLast(MakeSection([0, 8], [1, 6, 12]));
        document.AddLast(MakeSection([0, 9], [2, 7, 13]));
        document.AddLast(MakeSection([0, 10], [3, 8, 14]));
        document.AddLast(MakeSection([0, 11], [4, 9, 15]));


        var enumerator = document.GetEnumerator();
        var attempts = 0;
        InitializeJcMatrices();
        while (enumerator.MoveNext() && attempts < 10)
        {
            var req = enumerator.Current;
            if (!YuleAlgorithm.Handler(req, entities, NumDays, NumPeriods))
            {
                document.Remove(req);
                document.AddFirst(req);
                enumerator = document.GetEnumerator();
                InitializeJcMatrices();
                attempts++;
            }
        }
        
        PrintSchedule(document, entities);

        return;

        void InitializeJcMatrices()
        {
            foreach (var entity in entities) entity.AvailabilityMatrix = new SchedulingMatrix(NumDays, NumPeriods);
        }
    }
}
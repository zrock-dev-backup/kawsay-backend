using Api.Scheduling;
using Xunit;

namespace kawsay.Scheduling;

public class SchedulingAlgorithmTest
{
    const int NumDays = 5;
    const int NumPeriods = 26;
        
    private static SchedulingRequirementLine MakeMasterClass(List<int> ids)
    {
        var x = new SchedulingRequirementLine(2, 4, ids, NumDays, NumPeriods);
        x.SetZRange(1, 4, 0);
        x.SetZRange(6, 9, 0);
        x.SetZRange(12, 15, 0);
        return x;
    }

    private static SchedulingRequirementLine MakeSection(List<int> ids, List<int> periods)
    {
        var x = new SchedulingRequirementLine(1, 1, ids, NumDays, NumPeriods);
        foreach (var value in periods)
        {
            x.SetZ(value, 0);
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
            foreach (var keyValuePair in requirementLine.AssignedTimeslotList.TimeslotDict)
            {
                var schedule = keyValuePair.Value;
                periods += $" [{schedule[0]}, {schedule[1]}]";
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
            if (!SchedulingAlgorithm.Handler(req, entities, NumDays, NumPeriods))
            {
                document.Remove(req);
                document.AddFirst(req);
                enumerator = document.GetEnumerator();
                InitializeJcMatrices();
                attempts++;
            }
        }
        
        PrintSchedule(document, entities);;

        return;

        void InitializeJcMatrices()
        {
            foreach (var entity in entities) entity.AvailabilityMatrix = new SchedulingMatrix(NumDays, NumPeriods);
        }
    }
}
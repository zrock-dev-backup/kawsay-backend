// Services/SchedulingService.cs

using kawsay.Scheduling;
using KawsayApiMockup.Data;
using KawsayApiMockup.Entities;
using KawsayApiMockup.Scheduling;
using Microsoft.EntityFrameworkCore;
// Import the algorithm code namespace

namespace kawsay.Services
{
    // Service responsible for orchestrating the schedule generation process
    // using the Yule algorithm and interacting with the database.
    public class SchedulingService
    {
        private readonly KawsayDbContext _context;

        // Define the offset for ClassEntity IDs when mapping to SchedulingEntity IDs
        public const int ClassEntityIdOffset = 10000;

        public SchedulingService(KawsayDbContext context)
        {
            _context = context;
        }

        // Method to generate the schedule for a specific timetable
        // Returns true if scheduling was successful for all requirements, false otherwise.
        public async Task<bool> GenerateScheduleAsync(int timetableId)
        {
            System.Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");

            // 1. Fetch necessary data from the database
            var timetable = await _context.Timetables
                .Include(t => t.Days)
                .Include(t => t.Periods)
                .FirstOrDefaultAsync(t => t.Id == timetableId);

            if (timetable == null)
            {
                throw new ArgumentException($"Timetable with ID {timetableId} not found.");
            }

            // Fetch classes that need scheduling for this timetable
            var classesToSchedule = await _context.Classes
                .Include(c => c.Course) // Include Course/Teacher for Entity mapping
                .Include(c => c.Teacher)
                .Where(c => c.TimetableId == timetableId)
                .Where(c => c.RequiredOccurrenceCount > 0 &&
                            c.OccurrenceLength > 0) // Only schedule classes with requirements
                .ToListAsync();

            // Fetch all teachers (and potentially other resources like rooms)
            var allTeachers = await _context.Teachers.ToListAsync();
            // var allRooms = await _context.Rooms.ToListAsync(); // If rooms are added


            // 2. Prepare data for the Yule algorithm

            // Create the list of all Scheduling Entities (Teachers + Classes + potentially Rooms)
            var allSchedulingEntities = new List<SchedulingEntity>();
            // Add Teachers as entities
            allSchedulingEntities.AddRange(allTeachers.Select(t =>
                new SchedulingEntity(t.Id, t.Name, timetable.Days.Count, timetable.Periods.Count)));
            // Add Classes as entities (using offset for IDs)
            allSchedulingEntities.AddRange(classesToSchedule.Select(c =>
                new SchedulingEntity(c.Id + ClassEntityIdOffset, $"Class {c.Id} ({c.Course?.Code ?? "N/A"})",
                    timetable.Days.Count, timetable.Periods.Count)));


            // Initialize jC matrices for all scheduling entities at the start of each full attempt cycle
            // This is how the original algorithm handles backtracking - it clears the schedule and tries again
            Action initializeJcMatrices = () =>
            {
                foreach (var entity in allSchedulingEntities)
                {
                    // Re-initialize matrix to all 0s (available)
                    entity.jC = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
                }
                // TODO: Populate jC with any *fixed* unavailability constraints here if they exist
                // (e.g., teacher preferences, fixed room bookings, existing classes NOT being rescheduled)
                // This would involve querying the database for fixed bookings and marking the corresponding jC cells as 1.
            };


            // Create RequirementLine list using the Factory
            // Pass the list of classes to schedule, all entities, and timetable structure
            var requirementDocument = SchedulingDocumentFactory.GetDocument(
                classesToSchedule,
                allSchedulingEntities, // Pass the full list of entities
                timetable
            );
            
            // Check if there are any requirements to schedule
            if (!requirementDocument.Any())
            {
                System.Console.WriteLine("No requirements generated for scheduling. Clearing existing schedule.");
                // If no requirements, clear any existing occurrences for this timetable
                var existingOccurrences = await _context.ClassOccurrences
                    .Where(o => _context.Classes.Any(c => c.Id == o.ClassId && c.TimetableId == timetableId))
                    .ToListAsync();
                _context.ClassOccurrences.RemoveRange(existingOccurrences);
                await _context.SaveChangesAsync();
                return true; // Indicate successful generation (an empty schedule)
            }


            // 3. Run the Yule algorithm

            var attempts = 0;
            var maxAttempts = 100; // Prevent infinite loops (adjust as needed)

            // Use a while loop with the enumerator as in the original algorithm
            // The algorithm modifies the document order by moving failed requirements to the front.
            var currentDocument =
                new LinkedList<SchedulingRequirementLine>(requirementDocument); // Work on a copy for attempts
            var enumerator = currentDocument.GetEnumerator();


            // Reset jC matrices at the start of the first attempt cycle
            initializeJcMatrices();

            var allScheduledSuccessfully = true; // Assume success unless a requirement fails after attempts

            while (enumerator.MoveNext() && attempts < maxAttempts)
            {
                var currentReq = enumerator.Current;

                // The Handler function populates E based on current jC and Z, then tries to Schedule 'q' times
                if (!SchedulingAlgorithm.Handler(currentReq, allSchedulingEntities, timetable.Days.Count,
                        timetable.Periods.Count)) // Pass dimensions
                {
                    System.Console.WriteLine(
                        $"Scheduling failed for requirement S={string.Join(",", currentReq.S)} (q={currentReq.q}, len={currentReq.length}) after {attempts + 1} attempts. Moving to front.");
                    // If scheduling fails for a requirement, move it to the front and restart the loop
                    currentDocument.Remove(currentReq);
                    currentDocument.AddFirst(currentReq);
                    enumerator = currentDocument.GetEnumerator(); // Reset enumerator to start of the reordered list
                    initializeJcMatrices(); // Reset jC matrices for a new attempt cycle - crucial for backtracking!
                    attempts++; // Increment attempt counter
                    allScheduledSuccessfully = false; // Mark that at least one requirement failed
                }
                else
                {
                    System.Console.WriteLine(
                        $"Successfully scheduled requirement for S={string.Join(",", currentReq.S)}. Results: {currentReq.R.R.Count} occurrences scheduled.");
                    // If successful, the jC matrices of involved entities have been updated by UpdateEAvailability
                    // The algorithm proceeds to the next requirement in the list *without* resetting jC
                }
            }


            if (attempts >= maxAttempts)
            {
                System.Console.WriteLine(
                    $"Scheduling failed after {maxAttempts} attempts. Could not schedule all requirements.");
                // If we exit the loop because max attempts were reached, it means some requirements failed repeatedly.
                // The state of jC and R reflects the *last* attempt's partial schedule.
                // You might want to clear R for failed requirements or indicate partial success.
                // For now, we'll just return false indicating overall failure to schedule *all* requirements.
                return false;
            }

            // If the loop finished before max attempts, it means enumerator.MoveNext() is false.
            // This implies all requirements were processed.
            // If allScheduledSuccessfully is still true, then all were scheduled on the first pass through the loop(s).
            // If allScheduledSuccessfully is false, but we finished before max attempts, it means
            // requirements were reordered and eventually all scheduled in the later attempts.


            // 4. Translate algorithm output (RequirementLine.R) back to API Entities (ClassOccurrenceEntity)

            // Clear existing occurrences for the classes we attempted to schedule
            // This assumes you are regenerating the schedule for these classes from scratch
            var classIdsToSchedule = classesToSchedule.Select(c => c.Id).ToList();
            var existingOccurrences = await _context.ClassOccurrences
                .Where(o => classIdsToSchedule.Contains(o.ClassId))
                .ToListAsync();
            _context.ClassOccurrences.RemoveRange(existingOccurrences);


            var newOccurrences = new List<ClassOccurrenceEntity>();

            // Need sorted days and periods from the timetable structure to map indices back to IDs
            // Sort consistently with how the algorithm uses indices
            var sortedDays = timetable.Days.OrderBy(d => SchedulingAlgorithm.dayOrder.IndexOf(d.Name)).ToList();
            var sortedPeriods =
                timetable.Periods.OrderBy(p => dayjs(p.Start, "HH:mm").Ticks).ToList(); // Sort periods chronologically


            // Map algorithm results back to ClassOccurrenceEntity
            foreach (var requirement in currentDocument) // Iterate over the final state of the document
            {
                // Find the ClassEntity associated with this requirement line
                // Use the offset to find the original ClassEntity ID from the S list
                var classEntitySchedulingId = requirement.S.FirstOrDefault(id => id >= ClassEntityIdOffset);
                if (classEntitySchedulingId == 0)
                {
                    // This requirement didn't seem to be linked to a Class Entity via the offset.
                    // This could happen if the factory created requirements differently or if S contains other entity types.
                    // Log a warning and skip creating occurrences for this requirement.
                    System.Console.WriteLine(
                        $"Warning: Could not find Class Entity ID in S list (using offset {ClassEntityIdOffset}) for requirement S={string.Join(",", requirement.S)}. Skipping occurrence creation for this requirement.");
                    continue;
                }

                var classEntityId = classEntitySchedulingId - ClassEntityIdOffset; // Get original ClassEntity ID
                // No need to refetch classEntity here, just need its ID for the occurrence


                foreach (var resultPair in requirement.R.R.Values) // resultPair is List<int> {dayIndex, periodIndex}
                {
                    var dayIndex = resultPair[0];
                    var periodIndex = resultPair[1];

                    // Map algorithm's day/period index back to TimetableDay/Period ID
                    if (dayIndex < 0 || dayIndex >= sortedDays.Count || periodIndex < 0 ||
                        periodIndex >= sortedPeriods.Count)
                    {
                        System.Console.WriteLine(
                            $"Warning: Algorithm result indices out of bounds for Class Entity ID {classEntityId}: Day index {dayIndex} (max {sortedDays.Count - 1}), Period index {periodIndex} (max {sortedPeriods.Count - 1}). Skipping occurrence creation for this result.");
                        continue; // Skip invalid results
                    }

                    var dayEntity = sortedDays[dayIndex];
                    var startPeriodEntity = sortedPeriods[periodIndex];

                    newOccurrences.Add(new ClassOccurrenceEntity
                    {
                        ClassId = classEntityId, // Link to the correct ClassEntity ID
                        DayId = dayEntity.Id,
                        StartPeriodId = startPeriodEntity.Id,
                        Length = requirement.length // Use the length from the requirement line
                    });
                }

                System.Console.WriteLine(
                    $"Created {requirement.R.R.Count} new occurrences for Class Entity ID {classEntityId}.");
            }

            // 5. Save the new occurrences to the database
            _context.ClassOccurrences.AddRange(newOccurrences);
            await _context.SaveChangesAsync();

            System.Console.WriteLine(
                $"Finished schedule generation for timetable ID: {timetableId}. Overall success: {allScheduledSuccessfully}");

            // Return overall success status
            return allScheduledSuccessfully;
        }
    }
}
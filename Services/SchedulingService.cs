// Services/SchedulingService.cs

using KawsayApiMockup.Data;
using KawsayApiMockup.Entities;
using KawsayApiMockup.Scheduling; // Import the algorithm code namespace
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KawsayApiMockup.Services // Ensure correct namespace for your project
{
    // Service responsible for orchestrating the schedule generation process
    // using the Yule algorithm and interacting with the database.
    public class SchedulingService
    {
        private readonly KawsayDbContext _context;

        // Define a constant offset for ClassEntity IDs when mapping them to SchedulingEntity IDs.
        // This is to prevent potential ID conflicts if TeacherEntity and ClassEntity IDs overlap.
        public const int ClassEntityIdOffset = 10000; // Choose a sufficiently large offset

        public SchedulingService(KawsayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates a schedule for a specific timetable using the Yule algorithm.
        /// </summary>
        /// <param name="timetableId">The ID of the timetable to schedule.</param>
        /// <returns>True if a complete schedule (all requirements met) was generated within attempt limits, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown if the timetable with the given ID is not found.</exception>
        public async Task<bool> GenerateScheduleAsync(int timetableId)
        {
            System.Console.WriteLine($"Starting schedule generation for timetable ID: {timetableId}");

            // 1. Fetch necessary data from the database
            var timetable = await _context.Timetables
                .Include(t => t.Days) // Include related days and periods for dimensions and mapping
                .Include(t => t.Periods)
                .FirstOrDefaultAsync(t => t.Id == timetableId);

            if (timetable == null)
            {
                throw new ArgumentException($"Timetable with ID {timetableId} not found.");
            }

            // Fetch classes that need scheduling for this timetable
            // These are the classes whose requirements (q > 0, length > 0) will form the algorithm's input document.
            var classesToSchedule = await _context.Classes
                .Include(c => c.Course) // Include Course/Teacher for Entity mapping in Factory/Results
                .Include(c => c.Teacher)
                .Where(c => c.TimetableId == timetableId)
                .Where(c => c.RequiredOccurrenceCount > 0 &&
                            c.OccurrenceLength > 0) // Only schedule classes with defined requirements
                .ToListAsync();

            // Fetch all teachers (and potentially other resources like rooms if added)
            // These are the potential entities that can be involved in requirements and have availability constraints.
            var allTeachers = await _context.Teachers.ToListAsync();
            // var allRooms = await _context.Rooms.ToListAsync(); // If rooms are added


            // 2. Prepare data for the Yule algorithm's internal state

            // Create the list of all Scheduling Entities (Teachers + Classes + potentially Rooms)
            // These entities will have their jC matrices updated by the algorithm.
            var allSchedulingEntities = new List<SchedulingEntity>();
            // Add Teachers as entities. Use their DB ID directly.
            allSchedulingEntities.AddRange(allTeachers.Select(t =>
                new SchedulingEntity(t.Id, t.Name, timetable.Days.Count, timetable.Periods.Count)));
            // Add Classes as entities. Use an offset to create unique IDs for classes in the scheduling context.
            allSchedulingEntities.AddRange(classesToSchedule.Select(c =>
                new SchedulingEntity(c.Id + ClassEntityIdOffset, $"Class {c.Id} ({c.Course?.Code ?? "N/A"})",
                    timetable.Days.Count, timetable.Periods.Count)));

            // Initialize jC matrices for all scheduling entities at the start of each full attempt cycle.
            // This is crucial for the algorithm's backtracking. It clears the schedule state and potentially
            // repopulates fixed constraints.
            Action initializeJcMatrices = () =>
            {
                foreach (var entity in allSchedulingEntities)
                {
                    // Re-initialize matrix to all 0s (available)
                    entity.jC = new SchedulingMatrix(timetable.Days.Count, timetable.Periods.Count);
                }
                // TODO: Populate jC with any *fixed* unavailability constraints here if they exist.
                // (e.g., teacher preferences/unavailability, fixed room bookings, existing classes NOT being rescheduled).
                // This would involve querying the database for these fixed constraints and marking the corresponding jC cells as 1.
            };


            // Create RequirementLine list using the Factory.
            // The factory translates ClassEntity requirements into algorithm RequirementLines.
            var requirementDocument = SchedulingDocumentFactory.GetDocument(
                classesToSchedule, // Classes to convert into requirements
                allSchedulingEntities, // Pass the full list of entities so factory can find required ones by ID
                timetable // Pass timetable structure for dimensions
            );

            // Check if there are any requirements generated by the factory.
            if (!requirementDocument.Any())
            {
                System.Console.WriteLine(
                    "No requirements generated for scheduling. Clearing existing schedule for this timetable.");
                // If no classes need scheduling, clear any previously scheduled occurrences for this timetable.
                var existingOccurrences = await _context.ClassOccurrences
                    .Where(o => _context.Classes.Any(c => c.Id == o.ClassId && c.TimetableId == timetableId))
                    .ToListAsync();
                _context.ClassOccurrences.RemoveRange(existingOccurrences);
                await _context.SaveChangesAsync();
                return true; // Indicate successful generation (an empty schedule)
            }


            // 3. Run the Yule algorithm's main loop with backtracking.

            var attempts = 0;
            var maxAttempts =
                100; // Limit the number of backtracking attempts to prevent infinite loops. Adjust as needed.

            // The algorithm modifies the document order by moving failed requirements to the front.
            // We need to work on a mutable list (LinkedList) and reset the enumerator when backtracking occurs.
            var currentDocument =
                new LinkedList<SchedulingRequirementLine>(requirementDocument); // Start with the initial requirements
            var enumerator = currentDocument.GetEnumerator();

            // Reset jC matrices at the start of the very first attempt cycle.
            initializeJcMatrices();

            var allScheduledSuccessfully = true; // Flag to track if all requirements are eventually scheduled.

            // The main scheduling loop: iterates through requirements and backtracks on failure.
            while (enumerator.MoveNext() && attempts < maxAttempts)
            {
                var currentReq = enumerator.Current;

                // Attempt to schedule all 'q' occurrences for the current requirement using the algorithm Handler.
                // The Handler itself iterates 'q' times, calling Schedule for each occurrence.
                if (!SchedulingAlgorithm.Handler(currentReq, allSchedulingEntities, timetable.Days.Count,
                        timetable.Periods.Count))
                {
                    System.Console.WriteLine(
                        $"Scheduling failed for requirement S=[{string.Join(",", currentReq.S)}] (q={currentReq.q}, len={currentReq.length}) after {attempts + 1} attempts. Moving to front and backtracking.");

                    // If scheduling fails for a requirement, it means it couldn't find slots for all 'q' occurrences
                    // in the *current* state of the jC matrices.
                    // To backtrack:
                    // a) Move the failed requirement to the front of the document.
                    currentDocument.Remove(currentReq);
                    currentDocument.AddFirst(currentReq);
                    // b) Reset the enumerator to start iterating from the beginning of the (reordered) document.
                    enumerator = currentDocument.GetEnumerator();
                    // c) Reset the jC matrices of *all* scheduling entities. This clears the schedule state found so far
                    //    in this attempt cycle, allowing the algorithm to try scheduling requirements in a new order
                    //    starting from a clean slate (plus any fixed unavailability).
                    initializeJcMatrices();
                    // d) Increment the attempt counter.
                    attempts++;
                    // e) Mark that at least one requirement failed in an attempt cycle.
                    allScheduledSuccessfully = false;
                }
                else
                {
                    System.Console.WriteLine(
                        $"Successfully scheduled requirement for S=[{string.Join(",", currentReq.S)}]. Results: {currentReq.R.R.Count} occurrences scheduled.");
                    // If successful, the jC matrices of involved entities have been updated by UpdateEAvailability
                    // for the slots that were just scheduled. The algorithm proceeds to the next requirement
                    // in the list *without* resetting jC, building the schedule incrementally.
                }
            }

            // Check the outcome of the scheduling loop.
            if (attempts >= maxAttempts)
            {
                // If we exit the loop because max attempts were reached, it means some requirements
                // failed repeatedly even after reordering and backtracking.
                System.Console.WriteLine(
                    $"Scheduling failed after {maxAttempts} attempts. Could not schedule all requirements.");
                // The current state of 'currentDocument' and the entities' jC matrices reflects
                // the best schedule found within the attempts, which might be incomplete.
                // You might choose to save this partial schedule or discard it.
                // For now, we'll indicate overall failure if max attempts are reached.
                return false;
            }

            // If the loop finished because enumerator.MoveNext() is false, it means all requirements
            // in the final order of 'currentDocument' were processed.
            // If 'allScheduledSuccessfully' is true, it means all requirements were scheduled
            // on the very first pass through the initial 'requirementDocument'.
            // If 'allScheduledSuccessfully' is false, but we finished before max attempts, it means
            // requirements that failed initially were reordered and eventually all scheduled
            // successfully in one of the later attempt cycles.

            // The 'currentDocument' now contains the requirements in the final order,
            // and their 'R' properties contain the scheduled slots from the *successful* attempt cycle.


            // 4. Translate algorithm output (RequirementLine.R) back to API Entities (ClassOccurrenceEntity)

            // Clear existing occurrences for the classes we attempted to schedule.
            // This assumes you are regenerating the schedule for these classes from scratch.
            var classIdsToSchedule = classesToSchedule.Select(c => c.Id).ToList();
            var existingOccurrencesA = await _context.ClassOccurrences
                .Where(o => classIdsToSchedule.Contains(o.ClassId))
                .ToListAsync();
            _context.ClassOccurrences.RemoveRange(existingOccurrencesA);


            var newOccurrences = new List<ClassOccurrenceEntity>();

            // Need sorted days and periods from the timetable structure to map algorithm's 0-based indices back to database IDs.
            // Sort consistently with how the algorithm uses indices (dayOrder for days, chronological for periods).
            var sortedDays = timetable.Days.OrderBy(d => SchedulingAlgorithm.dayOrder.IndexOf(d.Name)).ToList();
            var sortedPeriods = timetable.Periods.OrderBy(p => System.TimeSpan.ParseExact(p.Start, "HH\\:mm", null))
                .ToList(); // Sort periods chronologically using TimeSpan

            // Map algorithm results (dayIndex, periodIndex from RequirementLine.R) back to ClassOccurrenceEntity.
            foreach (var requirement in currentDocument) // Iterate over the final state of the document
            {
                // Find the ClassEntity associated with this requirement line.
                // Use the offset to find the original ClassEntity ID from the S list.
                var classEntitySchedulingId = requirement.S.FirstOrDefault(id => id >= ClassEntityIdOffset);
                if (classEntitySchedulingId == 0)
                {
                    // This requirement didn't seem to be linked to a Class Entity via the offset.
                    // This could happen if the factory created requirements differently or if S contains other entity types.
                    // Log a warning and skip creating occurrences for this requirement.
                    System.Console.WriteLine(
                        $"Warning: Could not find Class Entity ID in S list (using offset {ClassEntityIdOffset}) for requirement S=[{string.Join(",", requirement.S)}]. Skipping occurrence creation for this requirement.");
                    continue;
                }

                var classEntityId = classEntitySchedulingId - ClassEntityIdOffset; // Get original ClassEntity ID
                // No need to refetch classEntity here, just need its ID for the occurrence


                // Iterate through the scheduled slots (R) for this requirement.
                foreach (var resultPair in requirement.R.R.Values) // resultPair is List<int> {dayIndex, periodIndex}
                {
                    var dayIndex = resultPair[0];
                    var periodIndex = resultPair[1];

                    // Map algorithm's 0-based day/period indices back to TimetableDay/Period database IDs.
                    if (dayIndex < 0 || dayIndex >= sortedDays.Count || periodIndex < 0 ||
                        periodIndex >= sortedPeriods.Count)
                    {
                        System.Console.WriteLine(
                            $"Warning: Algorithm result indices out of bounds for Class Entity ID {classEntityId}: Day index {dayIndex} (max {sortedDays.Count - 1}), Period index {periodIndex} (max {sortedPeriods.Count - 1}). Skipping occurrence creation for this result.");
                        continue; // Skip invalid results from the algorithm
                    }

                    var dayEntity = sortedDays[dayIndex]; // Get the DayEntity using the 0-based index
                    var startPeriodEntity = sortedPeriods[periodIndex]; // Get the PeriodEntity using the 0-based index

                    // Create the new ClassOccurrenceEntity
                    newOccurrences.Add(new ClassOccurrenceEntity
                    {
                        ClassId = classEntityId, // Link to the correct ClassEntity ID
                        DayId = dayEntity.Id, // Use the database ID
                        StartPeriodId = startPeriodEntity.Id, // Use the database ID
                        Length = requirement.length // Use the length from the requirement line
                    });
                }

                System.Console.WriteLine(
                    $"Created {requirement.R.R.Count} new occurrences for Class Entity ID {classEntityId}.");
            }

            // 5. Save the new occurrences to the database
            // This will insert all the newly created ClassOccurrenceEntity objects.
            _context.ClassOccurrences.AddRange(newOccurrences);
            await _context.SaveChangesAsync();

            System.Console.WriteLine(
                $"Finished schedule generation for timetable ID: {timetableId}. Overall success: {(attempts < maxAttempts)}");

            // Return true if the algorithm finished within the max attempts (implies a solution was found, although potentially partial if not all requirements were satisfied)
            // Returning `(attempts < maxAttempts)` is a simple success indicator. A more robust API might
            // return details about which requirements were scheduled vs. which failed.
            return attempts < maxAttempts;
        }
    }
}
// Scheduling/SchedulingDocumentFactory.cs

using System.Collections.Generic;
using KawsayApiMockup.Scheduling; // Use the new namespace
using KawsayApiMockup.Entities; // Import Entity types from your API project
using System.Linq; // Needed for LINQ extensions
using KawsayApiMockup.Services; // Import SchedulingService for the offset constant

namespace KawsayApiMockup.Scheduling // Ensure correct namespace for your project
{
    // Factory responsible for creating the list of SchedulingRequirementLine instances
    // based on the data fetched from the database for a specific timetable.
    public static class SchedulingDocumentFactory
    {
        /// <summary>
        /// Creates a linked list of SchedulingRequirementLine objects (the "document")
        /// and potentially populates the global list of SchedulingEntities based on
        /// database data for a specific timetable.
        /// </summary>
        /// <param name="classesToSchedule">List of ClassEntity objects to convert into requirements.</param>
        /// <param name="allSchedulingEntities">The pre-populated list of all potential SchedulingEntities (Teachers, Classes, etc.).</param>
        /// <param name="timetable">The TimetableEntity structure (days, periods) for dimensions.</param>
        /// <returns>A LinkedList of SchedulingRequirementLine objects representing the scheduling problem.</returns>
        public static LinkedList<SchedulingRequirementLine> GetDocument(
            List<ClassEntity> classesToSchedule, // Classes from the DB that need scheduling
            List<SchedulingEntity> allSchedulingEntities, // The pre-populated list of all entities (Teachers, Classes, etc.)
            TimetableEntity timetable // The timetable structure (days, periods)
        )
        {
            var document = new LinkedList<SchedulingRequirementLine>();

            // Map ClassEntity data to RequirementLine instances
            foreach (var classEntity in classesToSchedule)
            {
                 // Get the scheduling parameters from the ClassEntity
                 // These properties were added via migration
                 var requiredOccurrenceCount = classEntity.RequiredOccurrenceCount;
                 var occurrenceLength = classEntity.OccurrenceLength;

                 // Determine the entities involved (S list) for this requirement.
                 // This includes the Teacher Entity ID (if assigned)
                 // and the Class Entity ID (mapped to a SchedulingEntity ID using the offset).
                 var sEntityIdsForClass = new List<int>();

                 if (classEntity.TeacherId.HasValue)
                 {
                     // Add Teacher SchedulingEntity ID. We assume TeacherEntity.Id is used directly.
                     // Validate that the teacher entity exists in the global list (it should, as it's passed in)
                      if (allSchedulingEntities.Any(se => se.Id == classEntity.TeacherId.Value))
                      {
                         sEntityIdsForClass.Add(classEntity.TeacherId.Value);
                      } else {
                           // Log a warning if the teacher wasn't found in the provided entity list
                           System.Console.WriteLine($"Warning: Teacher ID {classEntity.TeacherId.Value} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping teacher for S list for this requirement.");
                      }
                 }

                 // Add Class SchedulingEntity ID. We use a predefined offset from the SchedulingService.
                 var classSchedulingEntityId = classEntity.Id + SchedulingService.ClassEntityIdOffset; // Use the offset defined in the Service
                 // Validate that the class entity exists in the global list (it should, as it's added in the Service before calling the factory)
                 if (allSchedulingEntities.Any(se => se.Id == classSchedulingEntityId))
                 {
                      sEntityIdsForClass.Add(classSchedulingEntityId);
                 } else {
                      // This is a more critical error if the class entity itself isn't in the global list.
                      System.Console.WriteLine($"Error: Class Scheduling Entity ID {classSchedulingEntityId} for Class {classEntity.Id} not found in global SchedulingEntities list. Skipping requirement creation for this class.");
                      continue; // Skip creating requirement if the class itself isn't a valid entity
                 }


                 // Get the final list of entity IDs for the 'S' property of the requirement line.
                 var sList = sEntityIdsForClass.ToList(); // Already filtered/validated above


                 // Only create a requirement if scheduling is actually needed (q > 0, length > 0)
                 // and there are entities involved (S list is not empty).
                 if (requiredOccurrenceCount > 0 && occurrenceLength > 0 && sList.Count > 0)
                 {
                      // Create a GenericSchedulingRequirementLine instance
                      // Pass the parameters derived from the ClassEntity and timetable dimensions
                      var requirement = new GenericSchedulingRequirementLine(
                          requiredOccurrenceCount, // Use ClassEntity.RequiredOccurrenceCount for 'q'
                          occurrenceLength,      // Use ClassEntity.OccurrenceLength for 'length'
                          sList, // Use the determined list of entity IDs for 'S'
                          timetable.Days.Count, // Pass the number of days
                          timetable.Periods.Count // Pass the number of periods
                      );
                      document.AddLast(requirement); // Add the created requirement to the document
                 } else {
                     // Log a warning if a class requirement is skipped due to missing data
                     System.Console.WriteLine($"Warning: Skipping requirement creation for Class {classEntity.Id} ({classEntity.Course?.Name ?? "Unknown Course"})" +
                                              $" due to zero required occurrences ({requiredOccurrenceCount})/length ({occurrenceLength}) or no involved entities (S list count: {sList.Count}).");
                 }
            }

            // The document now contains one generic requirement line for each class
            // that needs to be scheduled, based on its properties and associated entities.

            return document; // Return the linked list of requirements
        }
    }
}

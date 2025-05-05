// Data/MockData.cs
using KawsayApiMockup.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace KawsayApiMockup.Data
{
    public static class MockData
    {
        // Use Lists to simulate database tables
        public static List<Course> Courses { get; } = new List<Course>();
        public static List<Teacher> Teachers { get; } = new List<Teacher>();
        public static List<TimetableStructure> Timetables { get; } = new List<TimetableStructure>();
        public static List<Class> Classes { get; } = new List<Class>();

        // Simple ID counters
        private static int nextTimetableId = 1;
        private static int nextTimetableDayId = 100; // Start IDs higher to distinguish
        private static int nextTimetablePeriodId = 200; // Start IDs higher
        private static int nextCourseId = 300;
        private static int nextTeacherId = 400;
        private static int nextClassId = 500;
        private static int nextOccurrenceId = 600;


        static MockData()
        {
            // Seed initial data
            SeedData();
        }

        private static void SeedData()
        {
            // Seed Courses
            AddCourse(new Course { Name = "Programming 1", Code = "CSPR-101" });
            AddCourse(new Course { Name = "Programming 2", Code = "CSPR-124" });
            AddCourse(new Course { Name = "Linear Algebra", Code = "MATH-201" });
            AddCourse(new Course { Name = "Calculus I", Code = "MATH-101" });

            // Seed Teachers
            AddTeacher(new Teacher { Name = "Dave Smith", Type = "Professor" });
            AddTeacher(new Teacher { Name = "Jane Doe", Type = "Faculty Practitioner" });
            AddTeacher(new Teacher { Name = "Alice Johnson", Type = "Professor" });

            // Seed a sample Timetable (optional, can be created via API)
            // Seed a sample Class (optional, can be created via API)
        }

        // Helper methods to add data and assign IDs
        public static Course AddCourse(Course course)
        {
            course.Id = nextCourseId++;
            Courses.Add(course);
            return course;
        }

         public static Teacher AddTeacher(Teacher teacher)
        {
            teacher.Id = nextTeacherId++;
            Teachers.Add(teacher);
            return teacher;
        }

        public static TimetableStructure AddTimetable(CreateTimetableRequest request)
        {
            var newTimetable = new TimetableStructure
            {
                Id = nextTimetableId++,
                Name = request.Name,
                Days = request.Days.Select(d => new TimetableDay { Id = nextTimetableDayId++, Name = d }).ToList(),
                Periods = request.Periods.Select(p => new TimetablePeriod { Id = nextTimetablePeriodId++, Start = p.Start, End = p.End }).ToList()
            };
            Timetables.Add(newTimetable);
            return newTimetable;
        }

        public static Class AddClass(CreateClassRequest request)
        {
            // Find the associated Course and Teacher
            var course = Courses.FirstOrDefault(c => c.Id == request.CourseId);
            var teacher = request.TeacherId.HasValue ? Teachers.FirstOrDefault(t => t.Id == request.TeacherId.Value) : null;

            // Basic validation (a real API would do more)
            if (course == null) throw new System.ArgumentException($"Course with ID {request.CourseId} not found.");
            if (request.TeacherId.HasValue && teacher == null) throw new System.ArgumentException($"Teacher with ID {request.TeacherId.Value} not found.");
             var timetable = Timetables.FirstOrDefault(t => t.Id == request.TimetableId);
             if (timetable == null) throw new System.ArgumentException($"Timetable with ID {request.TimetableId} not found.");
             // Add more validation: check if dayId/periodId exist in the timetable, check for conflicts

            var newClass = new Class
            {
                Id = nextClassId++,
                TimetableId = request.TimetableId,
                Course = course, // Embed the found course
                Teacher = teacher, // Embed the found teacher (or null)
                Occurrences = request.Occurrences.Select(o => new ClassOccurrence
                {
                    Id = nextOccurrenceId++, // Assign ID to occurrence
                    DayId = o.DayId,
                    StartPeriodId = o.StartPeriodId,
                    Length = o.Length
                }).ToList()
            };

            Classes.Add(newClass);
            return newClass;
        }

        // Helper methods for Update/Delete (matching DTOs defined, but not used by current frontend)
        public static TimetableStructure? UpdateTimetable(int id, UpdateTimetableRequest request)
        {
            var timetable = Timetables.FirstOrDefault(t => t.Id == id);
            if (timetable == null) return null;

            timetable.Name = request.Name;
            // For a real update, you'd handle days and periods updates/replacements here
             timetable.Days = request.Days.Select(d => new TimetableDay { Id = nextTimetableDayId++, Name = d }).ToList(); // Simple replacement with new IDs
             timetable.Periods = request.Periods.Select(p => new TimetablePeriod { Id = nextTimetablePeriodId++, Start = p.Start, End = p.End }).ToList(); // Simple replacement with new IDs

            return timetable;
        }

         public static bool DeleteTimetable(int id)
         {
             var timetable = Timetables.FirstOrDefault(t => t.Id == id);
             if (timetable == null) return false;
             Timetables.Remove(timetable);
             // In a real app, you'd also delete all classes associated with this timetable
             Classes.RemoveAll(c => c.TimetableId == id);
             return true;
         }

        public static Class? UpdateClass(int id, UpdateClassRequest request)
        {
            var cls = Classes.FirstOrDefault(c => c.Id == id);
            if (cls == null) return null;

            // Find the associated Course and Teacher (re-fetch in case they changed)
            var course = Courses.FirstOrDefault(c => c.Id == request.CourseId);
            var teacher = request.TeacherId.HasValue ? Teachers.FirstOrDefault(t => t.Id == request.TeacherId.Value) : null;

             if (course == null) throw new System.ArgumentException($"Course with ID {request.CourseId} not found during update.");
             if (request.TeacherId.HasValue && teacher == null) throw new System.ArgumentException($"Teacher with ID {request.TeacherId.Value} not found during update.");
             // Validate timetableId matches the existing one if necessary
             if (cls.TimetableId != request.TimetableId) throw new System.ArgumentException($"Cannot change timetableId ({request.TimetableId}) for existing class ID {id}.");


            cls.Course = course;
            cls.Teacher = teacher;

            // Simple occurrence update: replace all occurrences with the new list
            // In a real app, you might match by occurrence ID to update/delete/add specifically
            cls.Occurrences = request.Occurrences.Select(o => new ClassOccurrence
            {
                 // If occurrence has an ID, keep it. If not, assign a new one (for new occurrences in the list)
                 Id = o.Id.HasValue && o.Id.Value > 0 ? o.Id.Value : nextOccurrenceId++,
                 DayId = o.DayId,
                 StartPeriodId = o.StartPeriodId,
                 Length = o.Length
            }).ToList();


            return cls;
        }

        public static bool DeleteClass(int id)
        {
            var cls = Classes.FirstOrDefault(c => c.Id == id);
            if (cls == null) return false;
            Classes.Remove(cls);
            return true;
        }
    }
}

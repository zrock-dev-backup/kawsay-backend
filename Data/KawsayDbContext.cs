// Data/KawsayDbContext.cs
using KawsayApiMockup.Entities;
using Microsoft.EntityFrameworkCore;

namespace KawsayApiMockup.Data
{
    public class KawsayDbContext : DbContext
    {
        public KawsayDbContext(DbContextOptions<KawsayDbContext> options) : base(options)
        {
        }

        // Define DbSets for your entities
        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<TeacherEntity> Teachers { get; set; }
        public DbSet<TimetableEntity> Timetables { get; set; }
        public DbSet<TimetableDayEntity> TimetableDays { get; set; } // DbSet for Days
        public DbSet<TimetablePeriodEntity> TimetablePeriods { get; set; } // DbSet for Periods
        public DbSet<ClassEntity> Classes { get; set; }
        public DbSet<ClassOccurrenceEntity> ClassOccurrences { get; set; } // DbSet for Occurrences


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships

            // Timetable has many Days, Periods, Classes
            modelBuilder.Entity<TimetableEntity>()
                .HasMany(t => t.Days)
                .WithOne(d => d.Timetable)
                .HasForeignKey(d => d.TimetableId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete days when timetable is deleted

            modelBuilder.Entity<TimetableEntity>()
                .HasMany(t => t.Periods)
                .WithOne(p => p.Timetable)
                .HasForeignKey(p => p.TimetableId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete periods when timetable is deleted

             modelBuilder.Entity<TimetableEntity>()
                .HasMany(t => t.Classes)
                .WithOne(c => c.Timetable)
                .HasForeignKey(c => c.TimetableId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete classes when timetable is deleted


            // Class has one Course, one optional Teacher, many Occurrences
            modelBuilder.Entity<ClassEntity>()
                .HasOne(c => c.Course)
                .WithMany(course => course.Classes)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a course if classes reference it

            modelBuilder.Entity<ClassEntity>()
                .HasOne(c => c.Teacher)
                .WithMany(teacher => teacher.Classes)
                .HasForeignKey(c => c.TeacherId)
                .IsRequired(false) // Teacher is optional
                .OnDelete(DeleteBehavior.SetNull); // Set TeacherId to null if teacher is deleted

            modelBuilder.Entity<ClassEntity>()
                .HasMany(c => c.Occurrences)
                .WithOne(o => o.Class)
                .HasForeignKey(o => o.ClassId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete occurrences when class is deleted


            // ClassOccurrence references Day and Period
            modelBuilder.Entity<ClassOccurrenceEntity>()
                 .HasOne(o => o.Day)
                 .WithMany(day => day.Occurrences) // Optional navigation on DayEntity
                 .HasForeignKey(o => o.DayId)
                 .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a day if occurrences are on it

            modelBuilder.Entity<ClassOccurrenceEntity>()
                 .HasOne(o => o.StartPeriod) // Use the renamed navigation property
                 .WithMany(period => period.Occurrences) // Optional navigation on PeriodEntity
                 .HasForeignKey(o => o.StartPeriodId)
                 .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a period if occurrences start at it


            // Seed initial data using EF Core's seeding feature
            // This replaces the static constructor seeding in MockData
            modelBuilder.Entity<CourseEntity>().HasData(
                new CourseEntity { Id = 1, Name = "Programming 1", Code = "CSPR-101" },
                new CourseEntity { Id = 2, Name = "Programming 2", Code = "CSPR-124" },
                new CourseEntity { Id = 3, Name = "Linear Algebra", Code = "MATH-201" },
                new CourseEntity { Id = 4, Name = "Calculus I", Code = "MATH-101" }
            );

             modelBuilder.Entity<TeacherEntity>().HasData(
                new TeacherEntity { Id = 1, Name = "Dave Smith", Type = "Professor" },
                new TeacherEntity { Id = 2, Name = "Jane Doe", Type = "Faculty Practitioner" },
                new TeacherEntity { Id = 3, Name = "Alice Johnson", Type = "Professor" }
            );

            // Add more seeding for timetables, classes, etc. if needed for initial data
            // For this mockup, we'll assume timetables/classes are created via API
        }
    }
}

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class KawsayDbContext(DbContextOptions<KawsayDbContext> options) : DbContext(options)
{
    // Create db tables
    public DbSet<CourseEntity> Courses { get; set; }
    public DbSet<TeacherEntity> Teachers { get; set; }
    public DbSet<TimetableEntity> Timetables { get; set; }
    public DbSet<TimetableDayEntity> TimetableDays { get; set; }
    public DbSet<TimetablePeriodEntity> TimetablePeriods { get; set; }
    public DbSet<ClassEntity> Classes { get; set; }
    public DbSet<PeriodPreferenceEntity> PeriodPreferences { get; set; }
    public DbSet<ClassOccurrenceEntity> ClassOccurrences { get; set; }
    public DbSet<StudentEntity> Students { get; set; }
    public DbSet<StudentModuleGrade> StudentModuleGrades { get; set; }
    public DbSet<EnrollmentEntity> Enrollments { get; set; }
    public DbSet<CohortEntity> Cohorts { get; set; }
    public DbSet<StudentGroupEntity> StudentGroups { get; set; }
    public DbSet<SectionEntity> Sections { get; set; }
    public DbSet<CoursePrerequisiteEntity> CoursePrerequisites { get; set; }
    public DbSet<HolidayEntity> Holidays { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimetableEntity>()
            .HasMany(t => t.Days)
            .WithOne(d => d.Timetable)
            .HasForeignKey(d => d.TimetableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimetableEntity>()
            .HasMany(t => t.Periods)
            .WithOne(p => p.Timetable)
            .HasForeignKey(p => p.TimetableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimetableEntity>()
            .HasMany(t => t.Classes)
            .WithOne(c => c.Timetable)
            .HasForeignKey(c => c.TimetableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClassEntity>()
            .HasOne(c => c.Teacher)
            .WithMany(teacher => teacher.Classes)
            .HasForeignKey(c => c.TeacherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // enum to string conversion
        modelBuilder.Entity<ClassEntity>()
            .Property(c => c.ClassType)
            .HasConversion<string>();

        modelBuilder.Entity<ClassEntity>()
            .HasMany(c => c.ClassOccurrences)
            .WithOne(o => o.Class)
            .HasForeignKey(o => o.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PeriodPreferenceEntity>()
            .HasOne(o => o.StartPeriod)
            .WithMany(period => period.PeriodPreferences)
            .HasForeignKey(o => o.StartPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodPreferenceEntity>()
            .HasOne(p => p.Day)
            .WithMany(d => d.PeriodPreferences)
            .HasForeignKey(p => p.DayId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClassOccurrenceEntity>()
            .HasOne(o => o.StartPeriod)
            .WithMany(period => period.Occurrences)
            .HasForeignKey(o => o.StartPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentEntity>()
            .Property(s => s.Standing)
            .HasConversion<string>();

        modelBuilder.Entity<StudentModuleGrade>(entity =>
        {
            entity.HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Course)
                .WithMany()
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(g => g.Timetable)
                .WithMany()
                .HasForeignKey(g => g.TimetableId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EnrollmentEntity>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EnrollmentEntity>()
            .HasOne(e => e.Class)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Hierarchy (Cohort -> Group -> Section)
        modelBuilder.Entity<CohortEntity>()
            .HasMany(c => c.StudentGroups)
            .WithOne(g => g.Cohort)
            .HasForeignKey(g => g.CohortId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentGroupEntity>()
            .HasMany(g => g.Sections)
            .WithOne(s => s.StudentGroup)
            .HasForeignKey(s => s.StudentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student assigned to a Section
        modelBuilder.Entity<SectionEntity>()
            .HasMany(s => s.Students)
            .WithOne(st => st.Section)
            .HasForeignKey(st => st.SectionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Class linked to Group or Section
        modelBuilder.Entity<ClassEntity>()
            .HasOne(c => c.StudentGroup)
            .WithMany(g => g.Masterclasses)
            .HasForeignKey(c => c.StudentGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ClassEntity>()
            .HasOne(c => c.Section)
            .WithMany(s => s.LabClasses)
            .HasForeignKey(c => c.SectionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Course Prerequisites (Self-referencing Many-to-Many)
        modelBuilder.Entity<CoursePrerequisiteEntity>()
            .HasOne(cp => cp.Course)
            .WithMany()
            .HasForeignKey(cp => cp.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoursePrerequisiteEntity>()
            .HasOne(cp => cp.PrerequisiteCourse)
            .WithMany()
            .HasForeignKey(cp => cp.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Holidays
        modelBuilder.Entity<HolidayEntity>()
            .HasOne(h => h.Timetable)
            .WithMany()
            .HasForeignKey(h => h.TimetableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
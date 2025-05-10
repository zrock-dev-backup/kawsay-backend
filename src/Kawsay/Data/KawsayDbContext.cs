using kawsay.Entities;
using Microsoft.EntityFrameworkCore;

namespace kawsay.Data;

public class KawsayDbContext(DbContextOptions<KawsayDbContext> options) : DbContext(options)
{
    public DbSet<CourseEntity> Courses { get; set; }
    public DbSet<TeacherEntity> Teachers { get; set; }
    public DbSet<TimetableEntity> Timetables { get; set; }
    public DbSet<TimetableDayEntity> TimetableDays { get; set; }
    public DbSet<TimetablePeriodEntity> TimetablePeriods { get; set; }
    public DbSet<ClassEntity> Classes { get; set; }
    public DbSet<PeriodPreference> ClassOccurrences { get; set; }


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
            .HasOne(c => c.Course)
            .WithMany(course => course.Classes)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClassEntity>()
            .HasOne(c => c.Teacher)
            .WithMany(teacher => teacher.Classes)
            .HasForeignKey(c => c.TeacherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ClassEntity>()
            .HasMany(c => c.PeriodPreferences)
            .WithOne(o => o.Class)
            .HasForeignKey(o => o.ClassId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<PeriodPreference>()
            .HasOne(o => o.Day)
            .WithMany(day => day.Occurrences)
            .HasForeignKey(o => o.DayId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodPreference>()
            .HasOne(o => o.StartPeriod)
            .WithMany(period => period.Occurrences)
            .HasForeignKey(o => o.StartPeriodId)
            .OnDelete(DeleteBehavior.Restrict);


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
    }
}
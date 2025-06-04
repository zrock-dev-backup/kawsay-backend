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

    // Relationship configurations
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
        
        modelBuilder.Entity<ClassOccurrenceEntity>()
            .HasOne(o => o.StartPeriod)
            .WithMany(period => period.Occurrences)
            .HasForeignKey(o => o.StartPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<ClassOccurrenceEntity>()
            .HasOne(o => o.DayEntity)
            .WithMany(period => period.Occurrences)
            .HasForeignKey(o => o.DayId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
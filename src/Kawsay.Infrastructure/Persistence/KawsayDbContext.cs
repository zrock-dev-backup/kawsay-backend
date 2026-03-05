using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Persistence;
public class KawsayDbContext(DbContextOptions<KawsayDbContext> options) : DbContext(options)
{
public DbSet<TimetableEntity> Timetables => Set<TimetableEntity>();
public DbSet<TimetableDayEntity> TimetableDays => Set<TimetableDayEntity>();
public DbSet<TimetablePeriodEntity> TimetablePeriods => Set<TimetablePeriodEntity>();
public DbSet<SubjectSelectionEntity> SubjectSelections => Set<SubjectSelectionEntity>();
public DbSet<TeacherAssignmentEntity> TeacherAssignments => Set<TeacherAssignmentEntity>();
public DbSet<TeacherAvailabilityEntity> TeacherAvailabilities => Set<TeacherAvailabilityEntity>();

public DbSet<StudentEnrollmentEntity> StudentEnrollments => Set<StudentEnrollmentEntity>();
public DbSet<StudentAvailabilityEntity> StudentAvailabilities => Set<StudentAvailabilityEntity>();
public DbSet<DeferredStudentEntity> DeferredStudents => Set<DeferredStudentEntity>();

public DbSet<CourseRequirementEntity> CourseRequirements => Set<CourseRequirementEntity>();
public DbSet<StagedPlacementEntity> StagedPlacements => Set<StagedPlacementEntity>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<TimetableEntity>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasMany(e => e.Days).WithOne(d => d.Timetable).HasForeignKey(d => d.TimetableId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(e => e.Periods).WithOne(p => p.Timetable).HasForeignKey(p => p.TimetableId).OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(e => e.SelectedSubjects).WithOne(s => s.Timetable).HasForeignKey(s => s.TimetableId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<TeacherAssignmentEntity>().HasKey(e => e.Id);
    modelBuilder.Entity<TeacherAvailabilityEntity>().HasKey(e => e.Id);
    modelBuilder.Entity<StudentEnrollmentEntity>().HasKey(e => e.Id);
    modelBuilder.Entity<StudentAvailabilityEntity>().HasKey(e => e.Id);
    modelBuilder.Entity<DeferredStudentEntity>().HasKey(e => e.Id);
    modelBuilder.Entity<CourseRequirementEntity>().HasKey(e => e.Id);
    modelBuilder.Entity<StagedPlacementEntity>().HasKey(e => e.Id);
}
}

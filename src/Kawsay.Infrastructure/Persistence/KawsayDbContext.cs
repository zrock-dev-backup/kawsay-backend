using Domain.Entities;
using Domain.Entities.availability;
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
    
    public DbSet<AvailabilityEntity> Availabilities => Set<AvailabilityEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimetableEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Days).WithOne(d => d.Timetable).HasForeignKey(d => d.TimetableId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Periods).WithOne(p => p.Timetable).HasForeignKey(p => p.TimetableId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.SelectedSubjects).WithOne(s => s.Timetable).HasForeignKey(s => s.TimetableId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AvailabilityEntity>(entity =>
        {
            entity.ToTable("availability"); // Matches your SQL comment
        
            entity.HasKey(e => e.Id);
        
            // Map columns to match snake_case DB standard
            entity.Property(e => e.EntityType).HasColumnName("entity_type");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
        
            // Save Enum as string in the database
            entity.Property(e => e.AvailabilityStatus)
                .HasColumnName("availability_status")
                .HasConversion<string>(); 
              
            entity.Property(e => e.Day).HasColumnName("day");
        
            // Ranges replaced the single 'slot'
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
        
            entity.Property(e => e.Status).HasColumnName("status");
        });
    }
}
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AcademicStructureRepository(KawsayDbContext context) : IAcademicStructureRepository
{
    public async Task<CohortEntity> AddCohortAsync(CohortEntity cohort)
    {
        await context.Cohorts.AddAsync(cohort);
        await context.SaveChangesAsync();
        return cohort;
    }

    public async Task<CohortEntity?> GetCohortByIdAsync(int cohortId)
    {
        return await context.Cohorts
            .Include(c => c.StudentGroups)
            .ThenInclude(g => g.Sections)
            .FirstOrDefaultAsync(c => c.Id == cohortId);
    }
    
    public async Task<List<CohortEntity>> GetCohortsByTimetableAsync(int timetableId)
    {
        return await context.Cohorts
            .Where(c => c.TimetableId == timetableId)
            .Include(c => c.StudentGroups)
            .ThenInclude(g => g.Sections)
            .ToListAsync();
    }

    public async Task<StudentGroupEntity> AddStudentGroupAsync(StudentGroupEntity studentGroup)
    {
        await context.StudentGroups.AddAsync(studentGroup);
        await context.SaveChangesAsync();
        return studentGroup;
    }

    public async Task<StudentGroupEntity?> GetStudentGroupByIdAsync(int groupId)
    {
        return await context.StudentGroups
            .Include(g => g.Sections)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<SectionEntity> AddSectionAsync(SectionEntity section)
    {
        await context.Sections.AddAsync(section);
        await context.SaveChangesAsync();
        return section;
    }

    public async Task<SectionEntity?> GetSectionWithStudentsAsync(int sectionId)
    {
        return await context.Sections
            .Include(s => s.Students)
            .FirstOrDefaultAsync(s => s.Id == sectionId);
    }
    
    public async Task AssignStudentToSectionAsync(int studentId, int sectionId)
    {
        var student = await context.Students.FindAsync(studentId);
        if (student != null)
        {
            student.SectionId = sectionId;
            await context.SaveChangesAsync();
        }
    }
}

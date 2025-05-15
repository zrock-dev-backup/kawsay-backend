using Application.Interfaces.Persistence;
using Application.Models;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class ClassOccurrenceRepository(KawsayDbContext context) : IClassOccurrenceRepository
{
    public void AddRangeAsync(List<ClassOccurrenceEntity> classOccurrences)
    {
        context.AddRange(classOccurrences);
        context.SaveChanges();
    }
}
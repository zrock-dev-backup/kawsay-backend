using Application.Models;
using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IClassOccurrenceRepository
{
   void AddRangeAsync(List<ClassOccurrenceEntity> classOccurrences);
}
using Domain.Entities;

namespace Domain.Interfaces;

public interface ISchedulable
{
    int Length { get; }
    ICollection<ClassOccurrenceEntity> ClassOccurrences { get; }
}

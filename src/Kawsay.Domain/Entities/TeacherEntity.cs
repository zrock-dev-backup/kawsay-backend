using System.ComponentModel.DataAnnotations;
namespace Domain.Entities;

public class TeacherEntity
{
    [Key] public int Id { get; set; } // Internal Proxy ID
    public string ExternalTeacherId { get; set; } = string.Empty; // Mapping key from Teacher API
    public ICollection<ClassEntity> Classes { get; set; } = new List<ClassEntity>();
}

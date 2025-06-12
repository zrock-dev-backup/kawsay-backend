using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class ClassTypeConfigurationEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public ClassType ClassType { get; set; }

    [Required]
    [Range(1, 10)]
    public int DefaultLength { get; set; }
}

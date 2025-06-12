using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class SetClassTypeConfigurationRequest
{
    [Required]
    public ClassTypeDto ClassType { get; set; }

    [Required]
    [Range(1, 10)]
    public int DefaultLength { get; set; }
}

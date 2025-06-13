using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class EnrollmentRequestDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int ClassId { get; set; }

    public bool Force { get; set; } = false;
}

public class AvailableClassDto : ClassDto
{
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public bool IsRetake { get; set; }
}

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

public class EnrollmentResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public DateTime EnrollmentDate { get; set; }
}

public class AvailableClassDto : ClassDto
{
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public bool IsRetake { get; set; }
    public int CurrentEnrollment { get; set; } // Added for clarity
}

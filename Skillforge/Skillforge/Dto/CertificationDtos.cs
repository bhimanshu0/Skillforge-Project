using System.ComponentModel.DataAnnotations;

namespace Skillforge.Dto;

/// <summary>
/// Request DTO for issuing a certification to an employee who passed a course assessment.
/// </summary>
public class IssueCertificationRequestDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int CourseId { get; set; }
}

/// <summary>
/// Response DTO returned after successfully issuing a certification.
/// </summary>
public class CertificationResponseDto
{
    public int CertificationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

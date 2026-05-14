using Skillforge.Dto;

namespace Skillforge.Service;

/// <summary>
/// Defines the business logic contract for certification issuance.
/// </summary>
public interface ICertificationService
{
    /// <summary>
    /// Issues a certification for an employee who has passed an assessment for the given course.
    /// Validates employee existence, course status, pass result, and duplicate guard before persisting.
    /// </summary>
    /// <param name="dto">Request containing EmployeeId and CourseId.</param>
    /// <returns>
    /// A tuple with Success indicating the outcome, ErrorMessage describing any failure,
    /// and the CertificationResponseDto on success.
    /// </returns>
    Task<(bool Success, string ErrorMessage, CertificationResponseDto? Result)> IssueCertificationAsync(IssueCertificationRequestDto dto);
    Task<List<CertificationResponseDto>> GetAllCertificationsAsync();
    Task<List<CertificationResponseDto>> GetMyCertificationsAsync(int employeeId);

    /// <summary>
    /// Auto-issues a certification on course completion. Skips assessment check.
    /// Idempotent — no-op if a cert already exists for this employee + course.
    /// </summary>
    Task AutoIssueCertificationAsync(int employeeId, int courseId, string courseTitle);
}

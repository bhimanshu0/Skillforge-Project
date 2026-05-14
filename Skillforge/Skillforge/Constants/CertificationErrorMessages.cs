namespace Skillforge.Constants;

/// <summary>
/// Centralised error message strings for certification business rule violations.
/// </summary>
public static class CertificationErrorMessages
{
    public const string EmployeeNotFound = "Employee not found.";
    public const string CourseNotFound = "Course not found.";
    public const string CourseNotLive = "Course is not live.";
    public const string AssessmentNotPassed = "Employee has not passed an assessment for this course.";
    public const string ActiveCertificationExists = "An active certification already exists for this employee and course.";
}

using Skillforge.Domain;
using Skillforge.Dto;
using Skillforge.Repository;

namespace Skillforge.Service;

/// <summary>
/// Implements the business logic for assessment management.
/// Validates course existence and live status before persisting a new assessment via the repository.
/// </summary>
public class AssessmentService : IAssessmentService
{
    private readonly IAssessmentRepository _assessmentRepository;

    public AssessmentService(IAssessmentRepository assessmentRepository)
    {
        _assessmentRepository = assessmentRepository;
    }

    /// <summary>
    /// Validates the target course and creates a new assessment if all rules pass.
    /// Returns a failure result if the course does not exist or is not currently live.
    /// </summary>
    /// <param name="dto">The assessment creation request containing CourseId, Type, and MaxScore.</param>
    /// <returns>
    /// A tuple with Success set to true and the new AssessmentId on success,
    /// or Success set to false with an ErrorMessage describing the validation failure.
    /// </returns>
    public async Task<(bool Success, string ErrorMessage, int AssessmentId)> CreateAssessmentAsync(CreateAssessmentRequestDto dto)
    {
        var course = await _assessmentRepository.GetCourseByIdAsync(dto.CourseId);

        if (course == null)
            return (false, "Course not found.", 0);

        if (!course.Status)
            return (false, "Course is not live.", 0);

        if (dto.PassingScore > dto.MaxScore)
            return (false, "Passing score cannot exceed max score.", 0);

        // If a ModuleId is provided, validate it belongs to the course
        if (dto.ModuleId.HasValue)
        {
            var module = await _assessmentRepository.GetModuleByIdAsync(dto.ModuleId.Value);
            if (module == null || module.CourseID != dto.CourseId)
                return (false, "Module not found in this course.", 0);
        }

        var assessment = new Assessment
        {
            CourseID     = dto.CourseId,
            ModuleID     = dto.ModuleId,
            Type         = dto.Type,
            MaxScore     = dto.MaxScore,
            PassingScore = dto.PassingScore,
            Date         = DateTime.Now
        };

        int assessmentId = await _assessmentRepository.CreateAssessmentAsync(assessment);
        return (true, null!, assessmentId);
    }

    public async Task<List<AssessmentResponseDto>> GetAllAssessmentsAsync()
    {
        var assessments = await _assessmentRepository.GetAllAssessmentsAsync();
        return assessments.Select(a => new AssessmentResponseDto
        {
            AssessmentId = a.AssessmentID,
            CourseId     = a.CourseID,
            CourseName   = a.Course?.Title ?? string.Empty,
            ModuleId     = a.ModuleID,
            ModuleName   = a.Module?.Title,
            Type         = a.Type.ToString(),
            MaxScore     = a.MaxScore,
            PassingScore = a.PassingScore,
            Date         = a.Date
        }).ToList();
    }

    public async Task<List<EmployeeAssessmentDto>> GetAssessmentsForEmployeeAsync(int employeeId)
    {
        var assessments = await _assessmentRepository.GetAssessmentsForEnrolledCoursesAsync(employeeId);
        var results     = await _assessmentRepository.GetResultsByEmployeeAsync(employeeId);
        var resultDict  = results.ToDictionary(r => r.AssessmentID);

        return assessments.Select(a =>
        {
            resultDict.TryGetValue(a.AssessmentID, out var result);
            return new EmployeeAssessmentDto
            {
                AssessmentId = a.AssessmentID,
                CourseId     = a.CourseID,
                CourseName   = a.Course?.Title ?? string.Empty,
                ModuleId     = a.ModuleID,
                ModuleName   = a.Module?.Title,
                Type         = a.Type.ToString(),
                MaxScore     = a.MaxScore,
                Date         = a.Date,
                IsDone       = result != null,
                ResultId     = result?.ResultID,
                Score        = result?.Score,
                ResultStatus = result?.Status.ToString()
            };
        }).ToList();
    }

    public async Task<List<EmployeeAssessmentDto>> GetAssessmentsByModuleAsync(int moduleId, int employeeId)
    {
        var assessments = await _assessmentRepository.GetAssessmentsForModuleAsync(moduleId);
        var results     = await _assessmentRepository.GetResultsByEmployeeAsync(employeeId);
        var resultDict  = results.ToDictionary(r => r.AssessmentID);

        return assessments.Select(a =>
        {
            resultDict.TryGetValue(a.AssessmentID, out var result);
            return new EmployeeAssessmentDto
            {
                AssessmentId = a.AssessmentID,
                CourseId     = a.CourseID,
                CourseName   = a.Course?.Title ?? string.Empty,
                ModuleId     = a.ModuleID,
                ModuleName   = a.Module?.Title,
                Type         = a.Type.ToString(),
                MaxScore     = a.MaxScore,
                Date         = a.Date,
                IsDone       = result != null,
                ResultId     = result?.ResultID,
                Score        = result?.Score,
                ResultStatus = result?.Status.ToString()
            };
        }).ToList();
    }
}

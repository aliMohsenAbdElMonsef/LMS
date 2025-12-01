using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Models.ViewModels.Enrollment;
using LMS.MVC.Services.Response;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IEnrollmentService
    {
        Task<BasicServiceResult> EnrollAsync(RequestErollmentintCourseViewModel vm);
        Task<BasicServiceResult> UnenrollAsync(RequestErollmentintCourseViewModel vm);
        Task<BasicServiceResult> ApproveEnrollmentAsync(ApproveStudentEnrollment vm);
        Task<BasicServiceResult> DenyEnrollmentAsync(ApproveStudentEnrollment vm);
        Task<ServiceResponseDTO<List<ReadEnrollmentViewModel>>> GetEnrollmentsAsync(EnrollmentManagementRequest model);
        Task<ServiceResponseDTO<List<ReadEnrollmentViewModel>>> GetStudentEnrollmentsAsync(string userId);
        Task<ServiceResponseDTO<List<ReadEnrollmentViewModel>>> GetInstructorEnrollmentsAsync(string userId);
        Task<bool> IsApprovedEnrollmentAsync(string userId, string courseId);
        Task<string> GetEnrollmentStatusAsync(string userId, string courseId);
    }
}

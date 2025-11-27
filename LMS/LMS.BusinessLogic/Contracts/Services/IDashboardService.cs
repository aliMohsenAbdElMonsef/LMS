using LMS.BusinessLogic.DTOs.Dashboard;
using LMS.BusinessLogic.DTOs.Responses;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IDashboardService
    {
        Task<ServiceResponseDTO<AdminDashboardDTO>> GetAdminDashboardAsync();
        Task<ServiceResponseDTO<InstructorDashboardDTO>> GetInstructorDashboardAsync(string instructorId);
        Task<ServiceResponseDTO<StudentDashboardDTO>> GetStudentDashboardAsync(string studentId);
        Task<ServiceResponseDTO<CourseStatsDTO>> GetCourseStatsAsync(string courseId);
    }
}

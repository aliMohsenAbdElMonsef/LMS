using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Dashboard;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponseDTO<AdminDashboardDTO>>> GetAdminDashboard()
        {
            var result = await _dashboardService.GetAdminDashboardAsync();
            return Ok(result);
        }

        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<InstructorDashboardDTO>>> GetInstructorDashboard()
        {
            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _dashboardService.GetInstructorDashboardAsync(instructorId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponseDTO<InstructorDashboardDTO>>> GetInstructorDashboardById(string instructorId)
        {
            var result = await _dashboardService.GetInstructorDashboardAsync(instructorId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<StudentDashboardDTO>>> GetStudentDashboard()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _dashboardService.GetStudentDashboardAsync(studentId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<StudentDashboardDTO>>> GetStudentDashboardById(string studentId)
        {
            var result = await _dashboardService.GetStudentDashboardAsync(studentId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponseDTO<CourseStatsDTO>>> GetCourseStats(string courseId)
        {
            var result = await _dashboardService.GetCourseStatsAsync(courseId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}

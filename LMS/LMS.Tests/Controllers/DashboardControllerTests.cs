using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Dashboard;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _mockDashboardService;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _mockDashboardService = new Mock<IDashboardService>();
            _controller = new DashboardController(_mockDashboardService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetAdminDashboard_ReturnsOk()
        {

            var responseDto = new ServiceResponseDTO<AdminDashboardDTO> { Success = true };

            _mockDashboardService.Setup(s => s.GetAdminDashboardAsync())
                .ReturnsAsync(responseDto);


            var result = await _controller.GetAdminDashboard();


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetInstructorDashboard_ReturnsOk_WhenSuccessful()
        {

            var responseDto = new ServiceResponseDTO<InstructorDashboardDTO> { Success = true };

            _mockDashboardService.Setup(s => s.GetInstructorDashboardAsync("user-id"))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetInstructorDashboard();


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetStudentDashboard_ReturnsOk_WhenSuccessful()
        {

            var responseDto = new ServiceResponseDTO<StudentDashboardDTO> { Success = true };

            _mockDashboardService.Setup(s => s.GetStudentDashboardAsync("user-id"))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetStudentDashboard();


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCourseStats_ReturnsOk_WhenSuccessful()
        {

            var courseId = "course1";
            var responseDto = new ServiceResponseDTO<CourseStatsDTO> { Success = true };

            _mockDashboardService.Setup(s => s.GetCourseStatsAsync(courseId))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetCourseStats(courseId);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

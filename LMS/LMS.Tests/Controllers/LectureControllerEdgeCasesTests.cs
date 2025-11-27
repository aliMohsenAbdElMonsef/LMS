using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class LectureControllerEdgeCasesTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<ILectureServices> _mockService;
        private readonly LectureController _controller;

        public LectureControllerEdgeCasesTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockService = new Mock<ILectureServices>();
            _mockUnitOfServices.Setup(u => u.Lectures).Returns(_mockService.Object);
            _controller = new LectureController(_mockUnitOfServices.Object);
        }

        private void SetUserRole(string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        [Fact]
        public async Task CreateLecture_EmptyTitle_ReturnsBadRequest()
        {
            SetUserRole("Instructor");
            var dto = new CreateLectureDTO { Title = "", LectureDate = DateTime.UtcNow.AddDays(1), CourseId = "course1", InstructorId = "instructor1", StartTime = TimeSpan.FromHours(9) };
            _controller.ModelState.AddModelError("Title", "Required");
            var result = await _controller.CreateLecture(dto);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badResult.Value);
        }

        [Fact]
        public async Task CreateLecture_PastDate_ReturnsBadRequest()
        {
            SetUserRole("Instructor");
            var dto = new CreateLectureDTO { Title = "Lecture", LectureDate = DateTime.UtcNow.AddDays(-1), CourseId = "course1", InstructorId = "instructor1", StartTime = TimeSpan.FromHours(9) };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateLectureDTO>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ServiceResponseDTO<GetLectureDTO> { Success = false, Message = "Invalid date" });
            
            var result = await _controller.CreateLecture(dto);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badResult.Value as ServiceResponseDTO<GetLectureDTO>;
            Assert.NotNull(response);
            Assert.Contains("Invalid date", response.Message);
        }

        [Fact]
        public async Task CreateLecture_ScheduleConflict_ReturnsBadRequest()
        {
            SetUserRole("Instructor");
            var dto = new CreateLectureDTO { Title = "Lecture", LectureDate = DateTime.UtcNow.AddDays(2), CourseId = "course1", InstructorId = "instructor1", StartTime = TimeSpan.FromHours(9) };
            _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateLectureDTO>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ServiceResponseDTO<GetLectureDTO> { Success = false, Message = "Schedule conflict" });
            
            var result = await _controller.CreateLecture(dto);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value as ServiceResponseDTO<GetLectureDTO>;
            Assert.NotNull(response);
            Assert.Contains("Schedule conflict", response.Message);
        }

    }
}

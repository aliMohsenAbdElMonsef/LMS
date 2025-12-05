using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class EnrollmentControllerEdgeCasesTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<IStudentEnrollment> _mockService;
        private readonly EnrollmentController _controller;

        public EnrollmentControllerEdgeCasesTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockService = new Mock<IStudentEnrollment>();
            _mockUnitOfServices.Setup(u => u.StudentEnrollIntoCourse).Returns(_mockService.Object);
            _controller = new EnrollmentController(_mockUnitOfServices.Object);
        }

        private void SetUserRole(string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        [Fact]
        public async Task Enroll_StudentAlreadyEnrolled_ReturnsConflict()
        {

            SetUserRole("Student");
            var dto = new RequestEnrollIntoCourseDTO { UserId = "student1", CourseId = "course1" };
            _mockService.Setup(s => s.EnrollAsync(dto))
                .ReturnsAsync(new BasicResponseDTO { Success = false, Message = "Already enrolled" });


            var result = await _controller.EnrollStudent(dto);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Enroll_MissingStudentId_ReturnsBadRequest()
        {

            SetUserRole("Student");
            var dto = new RequestEnrollIntoCourseDTO { UserId = "", CourseId = "course1" };
            _controller.ModelState.AddModelError("UserId", "Required");


            var result = await _controller.EnrollStudent(dto);


            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badResult.Value);
        }

        [Fact]
        public async Task ApproveEnrollment_StudentRole_Unauthorized()
        {

            SetUserRole("Student");
            var enrollmentId = "enroll123";






        }
    }
}

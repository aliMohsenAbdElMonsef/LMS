using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class CourseControllerEdgeCasesTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<ICourseServices> _mockService;
        private readonly CoursesController _controller;

        public CourseControllerEdgeCasesTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockService = new Mock<ICourseServices>();
            _mockUnitOfServices.Setup(u => u.Courses).Returns(_mockService.Object);
            _controller = new CoursesController(_mockUnitOfServices.Object);
        }

        private void SetUserRole(string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        [Fact]
        public async Task CreateCourse_BadRequest_WhenDtoInvalid()
        {

            SetUserRole("Admin");
            var dto = new CreateCourseDTO { Name = "", Description = null };
            _controller.ModelState.AddModelError("Name", "Required");


            var result = await _controller.CreateCourse(dto);


            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badResult.Value);
        }
    }
}

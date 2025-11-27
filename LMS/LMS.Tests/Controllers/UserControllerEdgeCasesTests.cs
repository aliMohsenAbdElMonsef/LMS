using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class UserControllerEdgeCasesTests
    {
        private readonly Mock<IUserServices> _mockService;
        private readonly UserController _controller;

        public UserControllerEdgeCasesTests()
        {
            _mockService = new Mock<IUserServices>();
            _controller = new UserController(_mockService.Object);
        }

        private void SetUserRole(string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        [Fact]
        public async Task Register_InvalidEmail_ReturnsBadRequest()
        {
            var dto = new SignUpDTO { Email = "invalid-email", Password = "StrongPass123!", FirstName = "Test" };
            _mockService.Setup(s => s.CreateUserAsync(dto))
                .ReturnsAsync(new CreateUserResponseDTO { Success = false, Message = "Invalid email format" });
            
            var result = await _controller.RegisterUser(dto);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badResult.Value);
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            var dto = new SignUpDTO { Email = "test@example.com", Password = "123", FirstName = "Test" };
            _mockService.Setup(s => s.CreateUserAsync(dto))
                .ReturnsAsync(new CreateUserResponseDTO { Success = false, Message = "Password too weak" });
            
            var result = await _controller.RegisterUser(dto);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badResult.Value);
        }
    }
}

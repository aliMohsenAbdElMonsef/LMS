using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Token;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class TokenControllerEdgeCasesTests
    {
        private readonly Mock<ITokenServices> _mockService;
        private readonly TokenController _controller;

        public TokenControllerEdgeCasesTests()
        {
            _mockService = new Mock<ITokenServices>();
            _controller = new TokenController(_mockService.Object);
        }

        private void SetUserRole(string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        [Fact]
        public async Task RefreshToken_MissingJwt_ReturnsUnauthorized()
        {
            // No Authorization header set
            var dto = new RefreshTokenDTO { RefreshToken = "someRefreshToken" };
            _mockService.Setup(s => s.RefreshAccessTokenAsync(dto.RefreshToken))
                .ReturnsAsync(new RefreshTokenResponseDTO { Success = false, Message = "Invalid token" });
            
            var result = await _controller.RefreshToken(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshToken_MalformedJwt_ReturnsUnauthorized()
        {
            // Simulate malformed token by not setting user claims
            var dto = new RefreshTokenDTO { RefreshToken = "malformedToken" };
            _mockService.Setup(s => s.RefreshAccessTokenAsync(dto.RefreshToken))
                .ReturnsAsync(new RefreshTokenResponseDTO { Success = false, Message = "Malformed token" });
            
            var result = await _controller.RefreshToken(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshToken_ExpiredRefreshToken_ReturnsUnauthorized()
        {
            SetUserRole("User");
            var dto = new RefreshTokenDTO { RefreshToken = "expiredToken" };
            _mockService.Setup(s => s.RefreshAccessTokenAsync(dto.RefreshToken))
                .ReturnsAsync(new RefreshTokenResponseDTO { Success = false, Message = "Refresh token expired" });
            
            var result = await _controller.RefreshToken(dto);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }
    }
}

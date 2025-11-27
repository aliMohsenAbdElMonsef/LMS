using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class TokenServicesTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IBlackListedTokensServices> _blacklistServiceMock;
        private readonly TokenServices _tokenServices;

        public TokenServicesTests()
        {
            _configMock = new Mock<IConfiguration>();
            
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            _blacklistServiceMock = new Mock<IBlackListedTokensServices>();

            _configMock.Setup(x => x["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly12345!");
            _configMock.Setup(x => x["Jwt:DurationInMinutes"]).Returns("60");
            _configMock.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
            _configMock.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");
            _configMock.Setup(x => x["Jwt:RefreshTokenExpiryDays"]).Returns("7");

            _tokenServices = new TokenServices(
                _configMock.Object,
                _userManagerMock.Object,
                _blacklistServiceMock.Object
            );
        }

        [Fact]
        public async Task GenerateAccessToken_ShouldReturnToken_WhenUserIsValid()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", Email = "test@example.com" };
            var roles = new List<string> { "Student" };

            // Act
            var (token, expires) = await _tokenServices.GenerateAccessToken(user, roles);

            // Assert
            Assert.NotNull(token);
            Assert.True(expires > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnToken_WhenCalled()
        {
            // Act
            var (refreshToken, expires) = _tokenServices.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.True(expires > DateTime.UtcNow);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnTrue_WhenTokenIsValid()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1" };
            var refreshToken = "valid_refresh_token";
            var expiryDate = DateTime.UtcNow.AddDays(1).ToString("O");

            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "LMS", "RefreshToken")).ReturnsAsync(refreshToken);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "LMS", "RefreshTokenExpiry")).ReturnsAsync(expiryDate);

            // Act
            var result = await _tokenServices.ValidateRefreshTokenAsync(user, refreshToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ShouldReturnFalse_WhenTokenIsExpired()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1" };
            var refreshToken = "expired_refresh_token";
            var expiryDate = DateTime.UtcNow.AddDays(-1).ToString("O");

            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "LMS", "RefreshToken")).ReturnsAsync(refreshToken);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "LMS", "RefreshTokenExpiry")).ReturnsAsync(expiryDate);

            // Act
            var result = await _tokenServices.ValidateRefreshTokenAsync(user, refreshToken);

            // Assert
            Assert.False(result);
        }
    }
}

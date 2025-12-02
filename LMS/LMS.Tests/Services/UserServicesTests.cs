using Application.DTOs.User;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Services
{
    public class UserServicesTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<ITokenServices> _tokenServicesMock;
        private readonly Mock<IBlackListedTokensServices> _blackListedTokensServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly UserServices _userServices;

        public UserServicesTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(roleStoreMock.Object, null, null, null, null);

            _tokenServicesMock = new Mock<ITokenServices>();
            _blackListedTokensServiceMock = new Mock<IBlackListedTokensServices>();
            _emailServiceMock = new Mock<IEmailService>();

            _userServices = new UserServices(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _tokenServicesMock.Object,
                _blackListedTokensServiceMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnSuccess_WhenUserIsCreated()
        {
            // Arrange
            var signUpDto = new SignUpDTO
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                ApplyAs = 0 // Student
            };

            var users = new List<ApplicationUser>().BuildMockDbSet().Object;
            _userManagerMock.Setup(x => x.Users).Returns(users);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userServices.CreateUserAsync(signUpDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User created successfully. Pending approval from admin.", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnFailure_WhenUserAlreadyExists()
        {
            // Arrange
            var signUpDto = new SignUpDTO
            {
                UserName = "existinguser",
                Email = "existing@example.com",
                Password = "Password123!",
                FirstName = "Existing",
                LastName = "User",
                ApplyAs = 0
            };

            var existingUser = new ApplicationUser { UserName = "existinguser", Email = "existing@example.com", IsDeleted = false };
            var users = new List<ApplicationUser> { existingUser }.BuildMockDbSet().Object;
            
            _userManagerMock.Setup(x => x.Users).Returns(users);

            // Act
            var result = await _userServices.CreateUserAsync(signUpDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("A user with this email or username already exists.", result.Message);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnSuccess_WhenCredentialsAreValidAndUserIsApproved()
        {
            // Arrange
            var loginDto = new LoginDTO { EmailOrUserName = "testuser", Password = "Password123!" };
            var user = new ApplicationUser 
            { 
                Id = "user1", 
                UserName = "testuser", 
                Email = "test@example.com", 
                Status = ApplicationStatus.Approved 
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(loginDto.EmailOrUserName)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });

            _tokenServicesMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
                .ReturnsAsync(("access_token", DateTime.UtcNow.AddHours(1)));
            _tokenServicesMock.Setup(x => x.GenerateRefreshToken())
                .Returns(("refresh_token", DateTime.UtcNow.AddDays(7)));

            // Act
            var result = await _userServices.LoginUser(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful", result.Message);
            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnFailure_WhenUserIsNotApproved()
        {
            // Arrange
            var loginDto = new LoginDTO { EmailOrUserName = "pendinguser", Password = "Password123!" };
            var user = new ApplicationUser 
            { 
                Id = "user2", 
                UserName = "pendinguser", 
                Status = ApplicationStatus.Pending 
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(loginDto.EmailOrUserName)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);

            // Act
            var result = await _userServices.LoginUser(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not approved by admin yet", result.Message);
        }

        [Fact]
        public async Task ApproveUserAsync_ShouldApproveUser_WhenUserExists()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId, Status = ApplicationStatus.Pending, ApplyAs = UserType.Student };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Student")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userServices.ApproveUserAsync(userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(ApplicationStatus.Approved, user.Status);
        }


    }
}

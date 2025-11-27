using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.MainEntities;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace LMS.Tests.Services
{
    public class BlackListedTokensServicesTests
    {
        private readonly Mock<IBlackListedTokens> _blackListedTokensRepoMock;
        private readonly BlackListedTokensServices _blackListedTokensService;

        public BlackListedTokensServicesTests()
        {
            _blackListedTokensRepoMock = new Mock<IBlackListedTokens>();
            _blackListedTokensService = new BlackListedTokensServices(_blackListedTokensRepoMock.Object);
        }

        [Fact]
        public async Task IsTokenBlackListedAsync_ShouldReturnFalse_WhenTokenNotFound()
        {
            // Arrange
            var token = "test_token";
            _blackListedTokensRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BlackListedTokens, bool>>>()))
                .ReturnsAsync((BlackListedTokens)null);

            // Act
            var result = await _blackListedTokensService.IsTokenBlackListedAsync(token);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsTokenBlackListedAsync_ShouldReturnFalse_WhenTokenIsExpired()
        {
            // Arrange
            var token = "expired_token";
            var expiredBlacklistedToken = new BlackListedTokens
            {
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
                UserId = "user1",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            _blackListedTokensRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BlackListedTokens, bool>>>()))
                .ReturnsAsync(expiredBlacklistedToken);

            // Act
            var result = await _blackListedTokensService.IsTokenBlackListedAsync(token);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsTokenBlackListedAsync_ShouldReturnTrue_WhenTokenIsBlacklistedAndNotExpired()
        {
            // Arrange
            var token = "valid_blacklisted_token";
            var blacklistedToken = new BlackListedTokens
            {
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(1), // Not expired
                UserId = "user1",
                CreatedAt = DateTime.UtcNow
            };

            _blackListedTokensRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BlackListedTokens, bool>>>()))
                .ReturnsAsync(blacklistedToken);

            // Act
            var result = await _blackListedTokensService.IsTokenBlackListedAsync(token);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddTokenAsync_ShouldAddTokenToBlacklist()
        {
            // Arrange
            var token = "new_token";
            var expiryDate = DateTime.UtcNow.AddDays(7);
            var userId = "user1";
            BlackListedTokens capturedToken = null;

            _blackListedTokensRepoMock.Setup(r => r.AddAsync(It.IsAny<BlackListedTokens>()))
                .Callback<BlackListedTokens>(t => capturedToken = t)
                .Returns(Task.CompletedTask);

            // Act
            await _blackListedTokensService.AddTokenAsync(token, expiryDate, userId);

            // Assert
            _blackListedTokensRepoMock.Verify(r => r.AddAsync(It.IsAny<BlackListedTokens>()), Times.Once);
            Assert.NotNull(capturedToken);
            Assert.Equal(token, capturedToken.Token);
            Assert.Equal(expiryDate, capturedToken.ExpiryDate);
            Assert.Equal(userId, capturedToken.UserId);
        }

        [Fact]
        public async Task RemoveExpiredTokensAsync_ShouldDeleteExpiredTokens()
        {
            // Arrange
            var expiredTokens = new List<BlackListedTokens>
            {
                new BlackListedTokens { Token = "expired1", ExpiryDate = DateTime.UtcNow.AddDays(-1), UserId = "user1" },
                new BlackListedTokens { Token = "expired2", ExpiryDate = DateTime.UtcNow.AddDays(-2), UserId = "user2" }
            };

            _blackListedTokensRepoMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<BlackListedTokens, bool>>>()))
                .ReturnsAsync(expiredTokens);
            _blackListedTokensRepoMock.Setup(r => r.DeleteAsync(It.IsAny<BlackListedTokens>()))
                .Returns(Task.CompletedTask);

            // Act
            await _blackListedTokensService.RemoveExpiredTokensAsync();

            // Assert
            _blackListedTokensRepoMock.Verify(r => r.DeleteAsync(It.IsAny<BlackListedTokens>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RemoveExpiredTokensAsync_ShouldNotDeleteWhenNoExpiredTokens()
        {
            // Arrange
            var emptyList = new List<BlackListedTokens>();

            _blackListedTokensRepoMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<BlackListedTokens, bool>>>()))
                .ReturnsAsync(emptyList);

            // Act
            await _blackListedTokensService.RemoveExpiredTokensAsync();

            // Assert
            _blackListedTokensRepoMock.Verify(r => r.DeleteAsync(It.IsAny<BlackListedTokens>()), Times.Never);
        }
    }
}

using Azure.Core;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Token;


namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ITokenServices
    {
        Task<(string token, DateTime expires)> GenerateAccessToken(ApplicationUser user, IList<string> roles);
        (string refreshToken, DateTime expires) GenerateRefreshToken();
        DateTime GetExpiryFromToken(string token);

        Task<RefreshTokenResponseDTO> RefreshAccessTokenAsync(string refreshToken, string userId);
        Task<bool> ValidateRefreshToken(string refreshToken, string userId);
        Task<bool> ValidateRefreshTokenAsync(ApplicationUser user, string refreshToken); 

        Task<bool> IsAccessTokenBlacklisted(string token);
        Task BlacklistAccessToken(string token, DateTime expiry, string userId);

        Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expiryDate);
    }
}

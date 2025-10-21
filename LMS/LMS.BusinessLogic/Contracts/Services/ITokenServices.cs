using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ITokenServices
    {
        string GenerateAccessToken(ApplicationUser user);
        Task<string> GenerateRefreshToken(string userId);
        DateTime GetExpiryFromToken(string token);
        Task<bool> ValidateRefreshToken(string refreshToken, string userId);
        Task<bool> IsAccessTokenBlacklisted(string token);
        Task BlacklistAccessToken(string token, DateTime expiry, string userId);
        Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expiryDate)
    }
}

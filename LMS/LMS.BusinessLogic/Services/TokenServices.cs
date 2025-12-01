using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LMS.BusinessLogic.Services
{
    public class TokenServices : ITokenServices
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlackListedTokensServices _blacklistService;

        public TokenServices(
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            IBlackListedTokensServices blacklistService)
        {
            _config = config;
            _userManager = userManager;
            _blacklistService = blacklistService;
        }

        // ---------------- ACCESS TOKEN ----------------
        public async Task<(string token, DateTime expires)> GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(user.FirstName))
                authClaims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            if (!string.IsNullOrEmpty(user.LastName))
                authClaims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            foreach (var role in roles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:DurationInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: expires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }


        public DateTime GetExpiryFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expClaim = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value;
            var expUnix = long.Parse(expClaim);

            return DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        }

        // --------------- BLACKLIST ----------------
        public async Task BlacklistAccessToken(string token, DateTime expiry, string userId)
        {
            await _blacklistService.AddTokenAsync(token, expiry, userId);
        }

        public Task<bool> IsAccessTokenBlacklisted(string token)
        {
            return _blacklistService.IsTokenBlackListedAsync(token);
        }

        // ---------------- REFRESH TOKEN ----------------
        public (string refreshToken, DateTime expires) GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var refreshToken = Convert.ToBase64String(randomBytes);
            var expiryDate = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenExpiryDays"]));

            return (refreshToken, expiryDate);
        }

        public async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expiryDate)
        {
            await _userManager.SetAuthenticationTokenAsync(user, "LMS", "RefreshToken", refreshToken);
            await _userManager.SetAuthenticationTokenAsync(user, "LMS", "RefreshTokenExpiry", expiryDate.ToString("O"));
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await ValidateRefreshTokenAsync(user, refreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(ApplicationUser user, string refreshToken)
        {
            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "LMS", "RefreshToken");
            var storedExpiryString = await _userManager.GetAuthenticationTokenAsync(user, "LMS", "RefreshTokenExpiry");

            if (storedToken == null || storedExpiryString == null)
                return false;

            if (storedToken != refreshToken)
                return false;

            if (!DateTime.TryParse(storedExpiryString, out var expiryDate))
                return false;

            return expiryDate > DateTime.UtcNow;
        }

        public async Task<RefreshTokenResponseDTO> RefreshAccessTokenAsync(string refreshToken, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new RefreshTokenResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var isValid = await ValidateRefreshTokenAsync(user, refreshToken);
            if (!isValid)
            {
                return new RefreshTokenResponseDTO
                {
                    Success = false,
                    Message = "Refresh token expired or invalid."
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var (newAccessToken, accessExpiry) = await GenerateAccessToken(user, roles);

            var (newRefreshToken, refreshExpiry) = GenerateRefreshToken();
            await SaveRefreshTokenAsync(user, newRefreshToken, refreshExpiry);

            return new RefreshTokenResponseDTO
            {
                Success = true,
                Message = "Token refreshed successfully.",
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = accessExpiry,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = refreshExpiry
            };
        }
    }
}

using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
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
        public TokenServices(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<(string token, DateTime expires)> GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            await Task.CompletedTask; // dummy await to match async signature
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var authSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_config["Jwt:DurationInMinutes"])
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: expires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (accessToken, expires);
        }


        public (string refreshToken, DateTime expires) GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            var refreshToken = Convert.ToBase64String(randomNumber);
            var expiryDate = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_config["Jwt:RefreshTokenExpiryDays"])
            );

            return (refreshToken, expiryDate);
        }
        public async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expiryDate)
        {
            // Save token
            await _userManager.SetAuthenticationTokenAsync(
                user,
                "LMS",                 // LoginProvider
                "RefreshToken",        // Name
                refreshToken           // Value
            );

            // Save expiry
            await _userManager.SetAuthenticationTokenAsync(
                user,
                "LMS",
                "RefreshTokenExpiry",
                expiryDate.ToString("O") // ISO 8601 format
            );
        }

    }
}

using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using System.IdentityModel.Tokens.Jwt;

namespace LMS.MVC.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;

        public TokenService(IHttpContextAccessor httpContextAccessor, HttpClient client)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = client;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                if (jwt.ValidTo > DateTime.UtcNow.AddSeconds(5))
                    return accessToken;
            }

            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var refreshResponse = await _client.PostAsJsonAsync("api/token/refresh", new { RefreshToken = refreshToken });
            if (!refreshResponse.IsSuccessStatusCode)
                return null;

            var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<LoginServiceResult>();
            if (refreshResult?.Success != true)
                return null;

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "AccessToken",
                refreshResult.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshResult.AccessTokenExpiresAt
                });

            if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
            {
                _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                    "RefreshToken",
                    refreshResult.RefreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = refreshResult.RefreshTokenExpiresAt
                    });
            }

            return refreshResult.AccessToken;
        }
    }
}

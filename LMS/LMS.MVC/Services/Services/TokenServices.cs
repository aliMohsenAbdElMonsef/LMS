using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LMS.MVC.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public TokenService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return null;

            var accessToken = context.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                if (IsTokenValid(accessToken))
                    return accessToken;
            }

            var refreshToken = context.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                await SignOutUserAsync();
                return null;
            }

            var userId = GetUserId();
            var refreshDto = new { RefreshToken = refreshToken, UserId = userId };

            var refreshResponse = await _httpClient.PostAsJsonAsync("api/token/refresh", refreshDto);
            if (!refreshResponse.IsSuccessStatusCode)
            {
                await SignOutUserAsync();
                return null;
            }

            var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<LoginServiceResult>();
            if (refreshResult?.Success != true)
            {
                await SignOutUserAsync();
                return null;
            }

            // Save new tokens
            SetTokenCookie("AccessToken", refreshResult.AccessToken, refreshResult.AccessTokenExpiresAt);
            if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                SetTokenCookie("RefreshToken", refreshResult.RefreshToken, refreshResult.RefreshTokenExpiresAt);

            return refreshResult.AccessToken;
        }

        private bool IsTokenValid(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwt.ValidTo > DateTime.UtcNow.AddSeconds(5);
            }
            catch
            {
                return false;
            }
        }

        public string? GetUserId()
        {
            var accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                    return jwt.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid" || c.Type == "UserId")?.Value;
                }
                catch { }
            }

            return _httpContextAccessor.HttpContext?.Request.Cookies["UserId"];
        }

        private void SetTokenCookie(string name, string value, DateTime? expires)
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                name,
                value,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = expires
                });
        }

        public async Task SignOutUserAsync()
        {
            ClearAuthCookies();
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        public void ClearAuthCookies()
        {
            var cookiesToDelete = new[] { "AccessToken", "RefreshToken", "UserId" };
            foreach (var cookieName in cookiesToDelete)
            {
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete(cookieName);
            }
        }
    }
}

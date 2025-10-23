using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
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
            Console.WriteLine($"[TokenService] GetAccessTokenAsync called");
            Console.WriteLine($"[TokenService] HttpContext is null: {_httpContextAccessor.HttpContext == null}");

            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("[TokenService] ❌ HttpContext is null!");
                return null;
            }

            var allCookies = _httpContextAccessor.HttpContext.Request.Cookies;
            Console.WriteLine($"[TokenService] Total cookies found: {allCookies.Count}");

            foreach (var cookie in allCookies)
            {
                Console.WriteLine($"[TokenService] Cookie: {cookie.Key} = {cookie.Value.Substring(0, Math.Min(20, cookie.Value.Length))}...");
            }

            var accessToken = _httpContextAccessor.HttpContext.Request.Cookies["AccessToken"];
            Console.WriteLine($"[TokenService] AccessToken from cookies: {(string.IsNullOrEmpty(accessToken) ? "NOT FOUND" : "FOUND")}");

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

                    // 🔍 DEBUG: Print all claims to see what's in the token
                    Console.WriteLine($"[TokenService] 🔍 JWT Claims Debug:");
                    foreach (var claim in jwt.Claims)
                    {
                        Console.WriteLine($"[TokenService]   {claim.Type} = {claim.Value}");
                    }

                    // Check specifically for roles
                    var roleClaims = jwt.Claims.Where(c =>
                        c.Type == ClaimTypes.Role ||
                        c.Type == "role" ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    ).ToList();

                    Console.WriteLine($"[TokenService] 🔍 Role claims found: {roleClaims.Count}");
                    foreach (var roleClaim in roleClaims)
                    {
                        Console.WriteLine($"[TokenService]   Role: {roleClaim.Value}");
                    }

                    // Check for Admin role specifically
                    bool hasAdminRole = roleClaims.Any(c => c.Value == "Admin");
                    Console.WriteLine($"[TokenService] 🔍 Has Admin role: {hasAdminRole}");

                    Console.WriteLine($"[TokenService] JWT parsed successfully, expires at: {jwt.ValidTo}");
                    Console.WriteLine($"[TokenService] Current time: {DateTime.UtcNow}");

                    if (jwt.ValidTo > DateTime.UtcNow.AddSeconds(5))
                    {
                        Console.WriteLine($"[TokenService] ✅ AccessToken is valid");
                        return accessToken;
                    }
                    Console.WriteLine($"[TokenService] ❌ AccessToken expired");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TokenService] ❌ Error parsing JWT: {ex.Message}");
                }
            }

            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
            Console.WriteLine($"[TokenService] RefreshToken from cookies: {(string.IsNullOrEmpty(refreshToken) ? "NOT FOUND" : "FOUND")}");

            if (string.IsNullOrEmpty(refreshToken))
            {
                Console.WriteLine("[TokenService] ❌ No refresh token found, cannot refresh access token");
                return null;
            }

            var userId = GetUserId();
            Console.WriteLine($"[TokenService] UserId: {userId}");

            var refreshDto = new
            {
                RefreshToken = refreshToken,
                UserId = userId
            };

            Console.WriteLine("[TokenService] Attempting to refresh token...");

            // Use the injected HttpClient directly
            var refreshResponse = await _httpClient.PostAsJsonAsync("api/token/refresh", refreshDto);
            Console.WriteLine($"[TokenService] Refresh response status: {refreshResponse.StatusCode}");

            if (!refreshResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("[TokenService] ❌ Token refresh failed");
                return null;
            }

            var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<LoginServiceResult>();
            if (refreshResult?.Success != true)
                return null;

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "AccessToken",
                refreshResult.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
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
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = refreshResult.RefreshTokenExpiresAt
                    });
            }

            return refreshResult.AccessToken;
        }

        public string? GetUserId()
        {
            var accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                    var claimUserId = jwt.Claims.FirstOrDefault(c =>
                        c.Type == "sub" || c.Type == "nameid" || c.Type == "UserId")?.Value;

                    if (!string.IsNullOrEmpty(claimUserId))
                        return claimUserId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TokenService] Error getting user ID from token: {ex.Message}");
                }
            }

            return _httpContextAccessor.HttpContext?.Request.Cookies["UserId"];
        }
    }
}
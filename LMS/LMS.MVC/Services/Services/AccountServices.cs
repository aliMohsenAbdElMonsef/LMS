using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace LMS.MVC.Services.Services
{
    internal class AccountServices : IAccountService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountServices> _logger;

        public AccountServices(HttpClient client, IHttpContextAccessor httpContextAccessor, ILogger<AccountServices> logger)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<LoginServiceResult> LoginUserAsync(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation($"Attempting login for user: {model.EmailOrUserName}");
                
                var response = await _client.PostAsJsonAsync("api/user/login", model);
                _logger.LogInformation($"Login response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Login failed with status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error content: {errorContent}");
                }
                
                var loginResult = await response.Content.ReadFromJsonAsync<LoginServiceResult>();

                if (loginResult?.Success != true || string.IsNullOrEmpty(loginResult.AccessToken))
                {
                    _logger.LogWarning("Login failed or token not received");
                    return loginResult ?? new LoginServiceResult { Success = false, Message = "Invalid response" };
                }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to false for development, true for production
                SameSite = SameSiteMode.Lax, // Lax works better for local development
                Expires = loginResult.AccessTokenExpiresAt,
                Path = "/", // Explicitly set path
                IsEssential = true // Mark as essential cookie
            };

            _httpContextAccessor.HttpContext!.Response.Cookies.Append("AccessToken", loginResult.AccessToken, cookieOptions);
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("RefreshToken", loginResult.RefreshToken, cookieOptions);
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("UserId", loginResult.User.Id, cookieOptions);

            _logger.LogInformation($"✅ Cookies set - AccessToken: {loginResult.AccessToken.Substring(0, 20)}..., RefreshToken: {loginResult.RefreshToken.Substring(0, 20)}..., UserId: {loginResult.User.Id}");
            _logger.LogInformation($"✅ Cookie options - HttpOnly: {cookieOptions.HttpOnly}, Secure: {cookieOptions.Secure}, SameSite: {cookieOptions.SameSite}");

            // Parse JWT for claims
            var handler = new JwtSecurityTokenHandler();
            var tokenObj = handler.ReadJwtToken(loginResult.AccessToken);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResult.User.Id),
                new Claim(ClaimTypes.Name, loginResult.User.username ?? string.Empty),
                new Claim(ClaimTypes.Email, loginResult.User.Email ?? string.Empty)
            };

            foreach (var role in loginResult.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.rememberMe,
                ExpiresUtc = loginResult.AccessTokenExpiresAt
            };

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            _logger.LogInformation("✅ User signed in via Cookie Authentication.");
            return loginResult;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during login");
                return new LoginServiceResult { Success = false, Message = "Network error: Unable to connect to server" };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout during login");
                return new LoginServiceResult { Success = false, Message = "Request timeout: Server is not responding" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return new LoginServiceResult { Success = false, Message = "An unexpected error occurred" };
            }
        }

        public async Task<RegisterUserResult> RegisterUserAsync(SignUpViewModel model)
        {
            var formContent = new MultipartFormDataContent
            {
                { new StringContent(model.FirstName ?? ""), "FirstName" },
                { new StringContent(model.LastName ?? ""), "LastName" },
                { new StringContent(model.Email ?? ""), "Email" },
                { new StringContent(model.UserName ?? ""), "UserName" },
                { new StringContent(model.Password ?? ""), "Password" },
                { new StringContent(model.ApplyAs.ToString()), "ApplyAs" }
            };

            if (model.UserImage != null && model.UserImage.Length > 0)
            {
                var stream = model.UserImage.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.UserImage.ContentType);
                formContent.Add(fileContent, "UserImage", model.UserImage.FileName);
            }

            var response = await _client.PostAsync("api/user/register", formContent);
            var serviceResult = await response.Content.ReadFromJsonAsync<RegisterUserResult>();

            return serviceResult ?? new RegisterUserResult
            {
                Success = false,
                Message = "Unknown error occurred"
            };
        }
    }
}

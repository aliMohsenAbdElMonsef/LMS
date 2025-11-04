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
                var response = await _client.PostAsJsonAsync("api/user/login", model);
                var loginResult = await response.Content.ReadFromJsonAsync<LoginServiceResult>();

                if (loginResult?.Success != true || string.IsNullOrEmpty(loginResult.AccessToken))
                    return loginResult ?? new LoginServiceResult { Success = false };

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(loginResult.AccessToken);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResult.User.Id),
                    new Claim(ClaimTypes.Name, loginResult.User.username ?? string.Empty),
                    new Claim(ClaimTypes.Email, loginResult.User.Email ?? string.Empty)
                };

                if (loginResult.Roles != null)
                {
                    foreach (var role in loginResult.Roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.rememberMe,
                    ExpiresUtc = loginResult.AccessTokenExpiresAt,
                    AllowRefresh = true,
                    RedirectUri = "/"
                };

                await _httpContextAccessor.HttpContext!.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                _logger.LogInformation("✅ User authenticated with cookie scheme");

                SetTokenCookies(loginResult);

                return loginResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login process failed");
                return new LoginServiceResult { Success = false, Message = "Error during login" };
            }
        }

        private void SetTokenCookies(LoginServiceResult loginResult)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = loginResult.AccessTokenExpiresAt,
                Path = "/"
            };

            _httpContextAccessor.HttpContext!.Response.Cookies.Append("AccessToken", loginResult.AccessToken, cookieOptions);
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("RefreshToken", loginResult.RefreshToken, cookieOptions);
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("UserId", loginResult.User.Id, cookieOptions);
        }

        public async Task<bool> LogoutUserAsync()
        {
            try
            {
                ClearAuthCookies();
                await SignOutAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private void ClearAuthCookies()
        {
            string[] cookies = { "AccessToken", "RefreshToken", "UserId" };
            foreach (var cookie in cookies)
                _httpContextAccessor.HttpContext!.Response.Cookies.Delete(cookie);
        }

        public async Task<RegisterUserResult> RegisterUserAsync(SignUpViewModel model)
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent(model.FirstName ?? ""), "FirstName" },
                { new StringContent(model.LastName ?? ""), "LastName" },
                { new StringContent(model.Email ?? ""), "Email" },
                { new StringContent(model.UserName ?? ""), "UserName" },
                { new StringContent(model.Password ?? ""), "Password" },
                { new StringContent(model.ApplyAs.ToString()), "ApplyAs" }
            };

            if (model.UserImage != null)
            {
                var file = new StreamContent(model.UserImage.OpenReadStream());
                file.Headers.ContentType = new MediaTypeHeaderValue(model.UserImage.ContentType);
                form.Add(file, "UserImage", model.UserImage.FileName);
            }

            var response = await _client.PostAsync("api/user/register", form);
            return await response.Content.ReadFromJsonAsync<RegisterUserResult>()
                   ?? new RegisterUserResult { Success = false, Message = "Unknown error" };
        }
    }
}

using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using System.Net.Http.Headers;

namespace LMS.MVC.Services.Services
{
    internal class AccountServices : BaseMVCServices, IAccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountServices(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginServiceResult> LoginUserAsync(LoginViewModel model)
        {
            var response = await _client.PostAsJsonAsync("api/user/login", model);
            var loginResult = await response.Content.ReadFromJsonAsync<LoginServiceResult>();
            if (loginResult?.Success != true)
                return loginResult;

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "AccessToken",
                loginResult.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = loginResult.AccessTokenExpiresAt
                });

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "RefreshToken",
                loginResult.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = loginResult.RefreshTokenExpiresAt
                });

            return loginResult;
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

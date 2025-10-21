using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

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

        public async Task<ServiceResult> LoginUserAsync(LoginViewModel model)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("api/user/login", model);
                var serviceResult = await response.Content.ReadFromJsonAsync<ServiceResult>();

                if (serviceResult != null && serviceResult.Success && !string.IsNullOrEmpty(serviceResult.Token))
                {
                    _httpContextAccessor.HttpContext?.Session.SetString("JWToken", serviceResult.Token);
                    _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                        "AuthToken",
                        serviceResult.Token,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddHours(1)
                        });

                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(serviceResult.Token);

                    var claims = jwtToken.Claims.ToList();

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await _httpContextAccessor.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties { IsPersistent = true });
                }

                return serviceResult ?? new ServiceResult
                {
                    Success = false,
                    Message = "Unknown error occurred"
                };
            }
            catch
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again."
                };
            }
        }

        //public Task<ServiceResult> LogoutUserAsync()
        //{
            
        //}

        public async Task<ServiceResult> RegisterUserAsync(SignUpViewModel model)
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

            try
            {
                var response = await _client.PostAsync("api/user/register", formContent);

                var serviceResult = await response.Content.ReadFromJsonAsync<ServiceResult>();

                return serviceResult ?? new ServiceResult
                {
                    Success = false,
                    Message = "Unknown error occurred"
                };
            }
            catch
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again."
                };
            }
        }
    }
}

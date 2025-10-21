using LMS.MVC.Models.ViewModels.User;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    internal class UserServices : BaseMVCServices, IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserServices(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
            : base(httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.GetAsync("/api/User/all");
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>();
            return users ?? new List<UserViewModel>();
        }
    }

}

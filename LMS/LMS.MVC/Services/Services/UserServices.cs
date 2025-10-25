using LMS.MVC.Models.ViewModels.User;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    internal class UserServices : BaseMVCServices, IUserService
    {
        public UserServices(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            var response = await _client.GetAsync("api/User/all");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed: {response.StatusCode} - {error}");
            }

            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>();
            return users ?? new List<UserViewModel>();
        }
    }
}

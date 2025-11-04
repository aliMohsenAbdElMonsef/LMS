using LMS.MVC.Models.ViewModels.User;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    internal class UserServices : BaseMVCServices, IUserService
    {
        public UserServices(HttpClient client, IHttpContextAccessor contextAccessor) : base(client, contextAccessor)
        {
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            try
            {
                var response = await _client.GetAsync("api/User/all");

                response.EnsureSuccessStatusCode();

                var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>();

                return users ?? new List<UserViewModel>();
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Request timeout: The server took too long to respond", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("HTTP request failed while fetching users", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred while fetching users", ex);
            }
        }
    }
}

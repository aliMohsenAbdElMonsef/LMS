using LMS.MVC.Models.ViewModels.User;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    internal class UserServices : BaseMVCServices, IUserService
    {
        public UserServices(HttpClient client)
            : base(client)
        {
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            try
            {
                Console.WriteLine($"[UserServices] Making API call to: {_client.BaseAddress}api/User/all");

                var response = await _client.GetAsync("api/User/all");
                Console.WriteLine($"[UserServices] Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[UserServices] Error: {error}");
                    throw new Exception($"API call failed: {response.StatusCode} - {error}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[UserServices] Response content length: {content.Length}");

                var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>();
                Console.WriteLine($"[UserServices] Deserialized users count: {users?.Count() ?? 0}");

                return users ?? new List<UserViewModel>();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[UserServices] ❌ Request timeout: {ex.Message}");
                throw new Exception("Request timeout: The server took too long to respond");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[UserServices] ❌ HTTP error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserServices] ❌ Unexpected error: {ex.Message}");
                throw;
            }
        }
    }
}
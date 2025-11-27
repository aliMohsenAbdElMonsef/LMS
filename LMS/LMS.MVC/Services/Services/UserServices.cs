using LMS.MVC.Models.ViewModels.User;
using LMS.MVC.Services.Contracts.Services;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.Extensions.Configuration;

namespace LMS.MVC.Services.Services
{
    internal class UserServices : BaseMVCServices, IUserService
    {
        private string _apiBaseUrl = "https://localhost:7033/";

        public UserServices(HttpClient client, IHttpContextAccessor contextAccessor, IConfiguration configuration) : base(client, contextAccessor)
        {
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7033/";
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            try
            {
                var response = await _client.GetAsync("api/User/all");

                response.EnsureSuccessStatusCode();

                var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>();
                
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        if (!string.IsNullOrEmpty(user.UserImage) && !user.UserImage.StartsWith("http"))
                        {
                            user.UserImage = $"{_apiBaseUrl.TrimEnd('/')}/{user.UserImage.TrimStart('/')}";
                        }
                    }
                }

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

        public async Task<ServiceResponseDTO<UserViewModel>> GetUserById(string id)
        {
            try
            {
                var response = await _client.GetAsync($"api/User/profile/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                     return new ServiceResponseDTO<UserViewModel>
                     {
                         Success = false,
                         Message = "User not found"
                     };
                }

                var user = await response.Content.ReadFromJsonAsync<UserViewModel>();
                
                if (user != null && !string.IsNullOrEmpty(user.UserImage) && !user.UserImage.StartsWith("http"))
                {
                    user.UserImage = $"{_apiBaseUrl.TrimEnd('/')}/{user.UserImage.TrimStart('/')}";
                }

                return new ServiceResponseDTO<UserViewModel>
                {
                    Success = true,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<UserViewModel>
                {
                    Success = false,
                    Message = $"Error fetching user: {ex.Message}"
                };
            }
        }
    }
}

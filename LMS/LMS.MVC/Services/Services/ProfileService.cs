using LMS.MVC.Models.ViewModels.Profile;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using System.Net.Http.Json;

namespace LMS.MVC.Services.Services
{
    internal class ProfileService : BaseMVCServices, IProfileService
    {
        public ProfileService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        private async Task<SuccessServiceResult<T>> ExecuteApiCallAsync<T>(Func<Task<SuccessServiceResult<T>>> apiCall)
        {
            try
            {
                return await apiCall();
            }
            catch (Exception ex)
            {
                return new SuccessServiceResult<T>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<SuccessServiceResult<ProfileViewModel>> GetProfileAsync(string userId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var profile = await GetAsync<ProfileViewModel>($"api/user/profile/{userId}");
                
                if (profile != null && !string.IsNullOrEmpty(profile.UserImage) && profile.UserImage.StartsWith("/"))
                {
                    profile.UserImage = $"{_client.BaseAddress?.ToString().TrimEnd('/')}{profile.UserImage}";
                }

                return new SuccessServiceResult<ProfileViewModel>
                {
                    Success = true,
                    Data = profile
                };
            });
        }

        public async Task<SuccessServiceResult<ProfileViewModel>> UpdateProfileAsync(string userId, UpdateProfileViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {

                var formData = new MultipartFormDataContent();
                
                formData.Add(new StringContent(model.Id), "Id");
                formData.Add(new StringContent(model.FirstName ?? ""), "FirstName");
                formData.Add(new StringContent(model.LastName ?? ""), "LastName");
                formData.Add(new StringContent(model.Email ?? ""), "Email");
                formData.Add(new StringContent(model.UserName ?? ""), "UserName");
                formData.Add(new StringContent(model.PhoneNumber ?? ""), "PhoneNumber");
                formData.Add(new StringContent(model.Bio ?? ""), "Bio");
                formData.Add(new StringContent(model.Country ?? ""), "Country");
                formData.Add(new StringContent(model.City ?? ""), "City");

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var fileContent = new StreamContent(model.ProfilePicture.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ProfilePicture.ContentType);
                    formData.Add(fileContent, "UserImage", model.ProfilePicture.FileName);
                }

                var profile = await PutAsync<ProfileViewModel>($"api/user/profile/update/{userId}", formData);
                
                if (profile != null && !string.IsNullOrEmpty(profile.UserImage) && profile.UserImage.StartsWith("/"))
                {
                    profile.UserImage = $"{_client.BaseAddress?.ToString().TrimEnd('/')}{profile.UserImage}";
                }

                return new SuccessServiceResult<ProfileViewModel>
                {
                    Success = true,
                    Data = profile,
                    Message = "Profile updated successfully"
                };
            });
        }

        public async Task<SuccessServiceResult<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var model = new
                {
                    UserId = userId,
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                };

                await PostAsync<object>("api/user/change-password", JsonContent.Create(model));
                
                return new SuccessServiceResult<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Password changed successfully"
                };
            });
        }

        public async Task<SuccessServiceResult<UserStatsViewModel>> GetUserStatsAsync(string userId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var stats = await GetAsync<UserStatsViewModel>($"api/user/stats/{userId}");
                return new SuccessServiceResult<UserStatsViewModel>
                {
                    Success = true,
                    Data = stats
                };
            });
        }
    }
}

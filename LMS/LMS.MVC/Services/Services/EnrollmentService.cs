using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Models.ViewModels.Enrollment;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;

namespace LMS.MVC.Services.Services
{
    internal class EnrollmentService : BaseMVCServices, IEnrollmentService
    {
        protected readonly HttpClient _client;
        protected readonly IHttpContextAccessor _contextAccessor;
        private string _baseApiUrl = "api/enrollment";
        private readonly ITokenService _tokenService;

        public EnrollmentService(HttpClient client, IHttpContextAccessor accessor, ITokenService tokenservice)
            : base(client, accessor)
        {
            _client = client;
            _contextAccessor = accessor;
            _tokenService = tokenservice;
        }

        private async Task<string?> GetValidTokenAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
                return token;

            _contextAccessor.HttpContext?.Response.Redirect("/Account/Login");
            return null;
        }
        private async Task<string> GetUserRoleAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return "";

            try
            {
                var response = await _client.GetAsync($"api/user/{userId}/role");
                if (!response.IsSuccessStatusCode)
                    return "";

                var content = await response.Content.ReadAsStringAsync();
                return content.Trim('"');
            }
            catch
            {
                return "";
            }
        }

        protected string? CurrentUserRole =>
            _contextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Role)?.Value;

        protected string? CurrentUserId =>
            _contextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public async Task<BasicServiceResult> EnrollAsync(RequestErollmentintCourseViewModel vm)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new BasicServiceResult { Success = false, Message = "User not authenticated" };

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new RequestEnrollIntoCourseDTO
            {
                UserId = vm.UserId ?? CurrentUserId,
                CourseId = vm.CourseId
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            try
            {
                return await SendRequestAsync<BasicServiceResult>(() =>
                    _client.PostAsync($"{_baseApiUrl}enroll", content));
            }
            catch (Exception ex)
            {
                return new BasicServiceResult { Success = false, Message = $"Enrollment failed: {ex.Message}" };
            }
        }

        public async Task<BasicServiceResult> UnenrollAsync(RequestErollmentintCourseViewModel vm)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new BasicServiceResult { Success = false, Message = "User not authenticated" };

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new RequestEnrollIntoCourseDTO
            {
                UserId = vm.UserId,
                CourseId = vm.CourseId
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            return await SendRequestAsync<BasicServiceResult>(() =>
                _client.PutAsync($"{_baseApiUrl}unenroll", content));
        }

        public async Task<BasicServiceResult> ApproveEnrollmentAsync(ApproveStudentEnrollment vm)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new BasicServiceResult { Success = false, Message = "User not authenticated" };

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new ApproveStudentEnrollment
            {
                UserId = vm.UserId,
                CourseId = vm.CourseId
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            try
            {
                var apiUrl = await GetUrl(vm.UserId);
                return await SendRequestAsync<BasicServiceResult>(() =>
                    _client.PostAsync($"{apiUrl}approve", content));
            }
            catch (Exception ex)
            {
                return new BasicServiceResult { Success = false, Message = $"Approve enrollment failed: {ex.Message}" };
            }
        }

        public async Task<BasicServiceResult> DenyEnrollmentAsync(ApproveStudentEnrollment vm)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new BasicServiceResult { Success = false, Message = "User not authenticated" };

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new UpdateStudentEnrollmentDTO
            {
                UserId = vm.UserId,
                CourseId = vm.CourseId
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            try
            {
                var apiUrl = await GetUrl(vm.UserId);
                return await SendRequestAsync<BasicServiceResult>(() =>
                    _client.PostAsync($"{apiUrl}deny", content));
            }
            catch (Exception ex)
            {
                return new BasicServiceResult { Success = false, Message = $"Deny enrollment failed: {ex.Message}" };
            }
        }

        private async Task<string> GetUrl(string userId)
        {
            var role = await GetUserRoleAsync(userId);
            return role == "Instructor" ? "api/enrollment/instructor/" : "api/enrollment/student/";
        }

        public async Task<ServiceResponseDTO<List<ReadEnrollmentViewModel>>> GetEnrollmentsAsync(EnrollmentManagementRequest model)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new ServiceResponseDTO<List<ReadEnrollmentViewModel>>
                {
                    Success = false,
                    Message = "User not authenticated"
                };

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(model.SelectedStatus))
                query["status"] = model.SelectedStatus;
            if (!string.IsNullOrEmpty(model.UserSearch))
                query["userSearch"] = model.UserSearch;
            if (!string.IsNullOrEmpty(model.CourseSearch))
                query["courseCode"] = model.CourseSearch;
            if (!string.IsNullOrEmpty(model.Role))
                query["role"] = model.Role;

            string url = $"api/enrollment/all-filtered?{query}";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new ServiceResponseDTO<List<ReadEnrollmentViewModel>>
                {
                    Success = false,
                    Message = "Failed to fetch enrollments from API."
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<List<ReadEnrollmentViewModel>>>();
            return result!;
        }
    }
}

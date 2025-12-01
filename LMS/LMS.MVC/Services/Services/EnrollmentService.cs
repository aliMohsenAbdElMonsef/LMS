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
        private string _baseApiUrl = "api/enrollment/";
        private string _rootUrl;
        private readonly ITokenService _tokenService;

        public EnrollmentService(HttpClient client, IHttpContextAccessor accessor, ITokenService tokenservice, string baseUrl)
            : base(client, accessor)
        {
            _client = client;
            _contextAccessor = accessor;
            _tokenService = tokenservice;
            _rootUrl = baseUrl.TrimEnd('/');
            _baseApiUrl = $"{_rootUrl}/api/enrollment/";
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
                var response = await _client.GetAsync($"{_rootUrl}/api/user/{userId}/role");
                if (!response.IsSuccessStatusCode)
                    return "";

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ServiceResponseDTO<string>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Data ?? "";
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
                var role = CurrentUserRole?.ToLower() ?? "student";
                return await SendRequestAsync<BasicServiceResult>(() =>
                    _client.PostAsync($"{_baseApiUrl}{role}/enroll", content));
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

            var role = CurrentUserRole?.ToLower() ?? "student";
            return await SendRequestAsync<BasicServiceResult>(() =>
                _client.PutAsync($"{_baseApiUrl}{role}/unenroll", content));
        }

        public async Task<BasicServiceResult> ApproveEnrollmentAsync(ApproveStudentEnrollment vm)
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
            return role == "Instructor" ? $"{_rootUrl}/api/enrollment/instructor/" : $"{_rootUrl}/api/enrollment/student/";
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

            string url = $"{_rootUrl}/api/enrollment/all-filtered?{query}";
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

        public async Task<ServiceResponseDTO<List<ReadEnrollmentViewModel>>> GetStudentEnrollmentsAsync(string userId)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = "User not authenticated" };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _client.GetAsync($"{_rootUrl}/api/enrollment/student/{userId}");
                if (!response.IsSuccessStatusCode)
                    return new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = "Failed to fetch enrollments." };

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ServiceResponseDTO<List<ReadEnrollmentViewModel>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = "Failed to deserialize response." };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponseDTO<List<ReadEnrollmentViewModel>>> GetInstructorEnrollmentsAsync(string userId)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = "User not authenticated" };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _client.GetAsync($"{_rootUrl}/api/enrollment/instructor/{userId}");
                if (!response.IsSuccessStatusCode)
                    return new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = "Failed to fetch enrollments." };

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ServiceResponseDTO<List<ReadEnrollmentViewModel>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = "Failed to deserialize response." };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<List<ReadEnrollmentViewModel>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<bool> IsApprovedEnrollmentAsync(string userId, string courseId)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var role = await GetUserRoleAsync(userId);
                var apiUrl = role == "Instructor"
                    ? $"{_rootUrl}/api/enrollment/instructor/get"
                    : $"{_rootUrl}/api/enrollment/student/get";

                var query = HttpUtility.ParseQueryString(string.Empty);
                query["userId"] = userId;
                query["courseId"] = courseId;

                var response = await _client.GetAsync($"{apiUrl}?{query}");
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ServiceResponseDTO<ReadEnrollmentViewModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data?.Status == "Approved";
            }
            catch
            {
                return false;
            }
        }


        public async Task<string> GetEnrollmentStatusAsync(string userId, string courseId)
        {
            var token = await GetValidTokenAsync();
            if (string.IsNullOrEmpty(token))
                return "None";

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var role = await GetUserRoleAsync(userId);
                var apiUrl = role == "Instructor"
                    ? $"{_rootUrl}/api/enrollment/instructor/get"
                    : $"{_rootUrl}/api/enrollment/student/get";

                var query = HttpUtility.ParseQueryString(string.Empty);
                query["userId"] = userId;
                query["courseId"] = courseId;

                var response = await _client.GetAsync($"{apiUrl}?{query}");
                if (!response.IsSuccessStatusCode)
                    return "None";

                var content = await response.Content.ReadAsStringAsync();
                
                // The API returns ServiceResponseDTO<ReadEnrollIntoCourseDTO>, not ServiceResponseDTO<string>
                var result = JsonSerializer.Deserialize<ServiceResponseDTO<ReadEnrollmentViewModel>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data?.Status ?? "None";
            }
            catch
            {
                return "None";
            }
        }
    }
}

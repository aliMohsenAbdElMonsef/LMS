using LMS.BusinessLogic.DTOs.Course;
using LMS.MVC.Services.Contracts.Services;
using System.Net.Http.Headers;

namespace LMS.MVC.Services.Services
{
    public class CourseService : ICourseService
    {
        private readonly HttpClient _client;
        private readonly ITokenService _tokenService;

        public CourseService(HttpClient client, ITokenService tokenService)
        {
            _client = client;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<ReadCourseDTO>> GetAllCoursesAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.GetAsync("api/course/all");
            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<IEnumerable<ReadCourseDTO>>();
                return courses ?? new List<ReadCourseDTO>();
            }

            return new List<ReadCourseDTO>();
        }

        public async Task<ReadCourseDTO?> GetCourseByIdAsync(string id)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.GetAsync($"api/course/{id}");
            if (response.IsSuccessStatusCode)
            {
                var course = await response.Content.ReadFromJsonAsync<ReadCourseDTO>();
                return course;
            }

            return null;
        }
    }
}

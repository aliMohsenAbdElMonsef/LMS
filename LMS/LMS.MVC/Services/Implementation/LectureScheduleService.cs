using LMS.BusinessLogic.DTOs.LectureSchedule;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LMS.MVC.Services.Implementation
{
    public class LectureScheduleService : ILectureScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;

        public LectureScheduleService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        private async Task AddAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<ServiceResponseDTO<GetLectureScheduleDTO>> CreateScheduleAsync(CreateLectureScheduleDTO dto)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/LectureSchedule", dto);
            return await response.Content.ReadFromJsonAsync<ServiceResponseDTO<GetLectureScheduleDTO>>();
        }

        public async Task<ServiceResponseDTO<bool>> DeleteScheduleAsync(string id)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/LectureSchedule/{id}");
            return await response.Content.ReadFromJsonAsync<ServiceResponseDTO<bool>>();
        }

        public async Task<ServiceResponseDTO<bool>> GenerateLecturesFromScheduleAsync(string scheduleId)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsync($"api/LectureSchedule/{scheduleId}/generate", null);
            return await response.Content.ReadFromJsonAsync<ServiceResponseDTO<bool>>();
        }

        public async Task<ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>>> GetCourseSchedulesAsync(string courseId)
        {
            await AddAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>>>($"api/LectureSchedule/course/{courseId}");
        }

        public async Task<ServiceResponseDTO<GetLectureScheduleDTO>> GetScheduleByIdAsync(string id)
        {
            await AddAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ServiceResponseDTO<GetLectureScheduleDTO>>($"api/LectureSchedule/{id}");
        }

        public async Task<ServiceResponseDTO<GetLectureScheduleDTO>> UpdateScheduleAsync(UpdateLectureScheduleDTO dto)
        {
            await AddAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/LectureSchedule/{dto.Id}", dto);
            return await response.Content.ReadFromJsonAsync<ServiceResponseDTO<GetLectureScheduleDTO>>();
        }
    }
}

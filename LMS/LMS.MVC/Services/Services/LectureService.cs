using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using LMS.MVC.Models.ViewModels.Lecture;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace LMS.MVC.Services.Services
{
    internal class LectureService : BaseMVCServices, ILectureService
    {
        public LectureService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
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

        public async Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetLecturesByCourseAsync(string courseId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var response = await GetAsync<ApiResponse<IEnumerable<LectureViewModel>>>($"api/Lecture/course/{courseId}");
                return new SuccessServiceResult<IEnumerable<LectureViewModel>>
                {
                    Success = response.Success,
                    Data = response.Data ?? Enumerable.Empty<LectureViewModel>(),
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> GetLectureByIdAsync(string id)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var response = await GetAsync<ApiResponse<LectureViewModel>>($"api/Lecture/get_lecture/{id}");
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> CreateLectureAsync(CreateLectureViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var response = await PostAsync<ApiResponse<LectureViewModel>>($"api/Lecture", JsonContent.Create(model));
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> UpdateLectureAsync(string id, UpdateLectureViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var response = await PutAsync<ApiResponse<LectureViewModel>>($"api/Lecture/{id}", JsonContent.Create(model));
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<bool>> TrackProgressAsync(string lectureId, int watchedSeconds)
        {
            // Mocking this for now as API endpoint is missing
            return await Task.FromResult(new SuccessServiceResult<bool>
            {
                Success = true,
                Data = true
            });
        }

        public async Task<bool> DeleteLectureAsync(string id)
        {
            await AttachAccessTokenAsync();
            var response = await _client.DeleteAsync($"api/Lecture/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<SuccessServiceResult<bool>> LaunchLectureAsync(string lectureId, string zoomLink)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await PostAsync<ApiResponse<bool>>($"api/Lecture/{lectureId}/launch", JsonContent.Create(new { ZoomLink = zoomLink }));
                return new SuccessServiceResult<bool>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<bool>> RescheduleLectureAsync(string lectureId, DateTime newDate, TimeSpan newStartTime)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                
                // First, fetch the existing lecture to get all its data
                var getLectureResponse = await GetLectureByIdAsync(lectureId);
                if (!getLectureResponse.Success || getLectureResponse.Data == null)
                {
                    return new SuccessServiceResult<bool> 
                    { 
                        Success = false, 
                        Message = getLectureResponse.Message ?? "Failed to fetch lecture data" 
                    };
                }
                
                var existingLecture = getLectureResponse.Data;
                
                // Calculate the new EndTime based on the existing duration
                var existingDuration = existingLecture.DurationMinutes;
                var newEndTime = newStartTime.Add(TimeSpan.FromMinutes(existingDuration));
                
                // Create UpdateLectureDTO with all existing data, but update date and time
                var dto = new 
                { 
                    Id = lectureId,
                    Title = existingLecture.Title,
                    Description = existingLecture.Description,
                    LectureDate = newDate, 
                    StartTime = newStartTime,
                    EndTime = newEndTime, // Preserve the duration
                    ZoomLink = existingLecture.ZoomLink
                };
                
                var response = await _client.PutAsJsonAsync($"api/Lecture/{lectureId}", dto);
                
                if (response.IsSuccessStatusCode)
                {
                    return new SuccessServiceResult<bool> { Success = true, Data = true, Message = "Lecture rescheduled successfully" };
                }
                
                var content = await response.Content.ReadAsStringAsync();
                return new SuccessServiceResult<bool> { Success = false, Message = content };
            });
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
            public List<string> Errors { get; set; }
        }

        public async Task<SuccessServiceResult<bool>> CheckLectureConflictAsync(string courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, string excludeLectureId = null)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var queryString = $"?courseId={courseId}&date={date:yyyy-MM-dd}&startTime={startTime}&endTime={endTime}";
                if (!string.IsNullOrEmpty(excludeLectureId))
                {
                    queryString += $"&excludeLectureId={excludeLectureId}";
                }

                var response = await _client.GetAsync($"api/Lecture/check-conflict{queryString}");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                
                return new SuccessServiceResult<bool> 
                { 
                    Success = result.Success, 
                    Data = result.Data, 
                    Message = result.Message 
                };
            });
        }

        public async Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetInstructorLecturesAsync(string instructorId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await GetAsync<ApiResponse<IEnumerable<LectureViewModel>>>($"api/Lecture/instructor/{instructorId}");
                return new SuccessServiceResult<IEnumerable<LectureViewModel>>
                {
                    Success = response.Success,
                    Data = response.Data ?? Enumerable.Empty<LectureViewModel>(),
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetUpcomingLecturesAsync(string courseId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await GetAsync<ApiResponse<IEnumerable<LectureViewModel>>>($"api/Lecture/upcoming/course/{courseId}");
                return new SuccessServiceResult<IEnumerable<LectureViewModel>>
                {
                    Success = response.Success,
                    Data = response.Data ?? Enumerable.Empty<LectureViewModel>(),
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetTodayLecturesAsync(string courseId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await GetAsync<ApiResponse<IEnumerable<LectureViewModel>>>($"api/Lecture/today/course/{courseId}");
                return new SuccessServiceResult<IEnumerable<LectureViewModel>>
                {
                    Success = response.Success,
                    Data = response.Data ?? Enumerable.Empty<LectureViewModel>(),
                    Message = response.Message
                };
            });
        }


        public async Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetMyLecturesAsync(string userId, string userRole)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await GetAsync<ApiResponse<IEnumerable<LectureViewModel>>>("api/Lecture/my-lectures");
                return new SuccessServiceResult<IEnumerable<LectureViewModel>>
                {
                    Success = response.Success,
                    Data = response.Data ?? Enumerable.Empty<LectureViewModel>(),
                    Message = response.Message
                };
            });
        }
    }
}

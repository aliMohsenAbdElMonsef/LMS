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
                var dto = new
                {
                    model.CourseId,
                    model.Title,
                    model.Description,
                    model.LectureDate,
                    model.StartTime,
                    model.EndTime,
                    model.DurationMinutes,
                    model.LectureNumber,
                    model.InstructorId
                };

                await AttachAccessTokenAsync();
                var response = await _client.PostAsJsonAsync("api/Lecture", dto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LectureViewModel>>();

                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = result.Success,
                    Data = result.Data,
                    Message = result.Message
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> UpdateLectureAsync(string id, UpdateLectureViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.Id), nameof(model.Id));
                if (model.Title != null) content.Add(new StringContent(model.Title), nameof(model.Title));
                if (model.Description != null) content.Add(new StringContent(model.Description), nameof(model.Description));
                if (model.LectureDate != default) content.Add(new StringContent(model.LectureDate.ToString("O")), nameof(model.LectureDate));
                if (model.StartTime != default) content.Add(new StringContent(model.StartTime.ToString()), nameof(model.StartTime));
                if (model.EndTime.HasValue) content.Add(new StringContent(model.EndTime.Value.ToString()), nameof(model.EndTime));
                if (model.InstructorId != null) content.Add(new StringContent(model.InstructorId), nameof(model.InstructorId));

                if (model.NewRecordingFile != null)
                {
                    var fileContent = new StreamContent(model.NewRecordingFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.NewRecordingFile.ContentType);
                    content.Add(fileContent, nameof(model.NewRecordingFile), model.NewRecordingFile.FileName);
                }

                if (model.NewMaterialsFile != null)
                {
                    var fileContent = new StreamContent(model.NewMaterialsFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.NewMaterialsFile.ContentType);
                    content.Add(fileContent, nameof(model.NewMaterialsFile), model.NewMaterialsFile.FileName);
                }

                await AttachAccessTokenAsync();
                var response = await _client.PutAsync($"api/Lecture/{id}", content);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LectureViewModel>>();

                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = result.Success,
                    Data = result.Data,
                    Message = result.Message
                };
            });
        }

        public async Task<SuccessServiceResult<bool>> TrackProgressAsync(string lectureId, int watchedSeconds)
        {

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
                

                var existingDuration = existingLecture.DurationMinutes;
                var newEndTime = newStartTime.Add(TimeSpan.FromMinutes(existingDuration));
                

                var dto = new 
                { 
                    Id = lectureId,
                    Title = existingLecture.Title,
                    Description = existingLecture.Description,
                    LectureDate = newDate, 
                    StartTime = newStartTime,
                    EndTime = newEndTime,
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

        public async Task<SuccessServiceResult<AttendanceStatisticsViewModel>> GetAttendanceStatisticsAsync(string userId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await GetAsync<ApiResponse<AttendanceStatisticsViewModel>>($"api/Lecture/attendance-statistics/{userId}");
                return new SuccessServiceResult<AttendanceStatisticsViewModel>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<AttendanceStatisticsViewModel>> GetCourseAttendanceStatisticsAsync(string courseId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await GetAsync<ApiResponse<AttendanceStatisticsViewModel>>($"api/Lecture/attendance/course/{courseId}");
                return new SuccessServiceResult<AttendanceStatisticsViewModel>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<bool>> JoinLectureAsync(string lectureId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                await AttachAccessTokenAsync();
                var response = await PostAsync<ApiResponse<bool>>($"api/Lecture/{lectureId}/join", null);
                return new SuccessServiceResult<bool>
                {
                    Success = response.Success,
                    Data = response.Data,
                    Message = response.Message
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> UploadLectureContentAsync(string lectureId, IFormFile? recording, IFormFile? materials, string userId, string userRole)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(userId), "userId");
                content.Add(new StringContent(userRole), "userRole");

                if (recording != null)
                {
                    var recordingContent = new StreamContent(recording.OpenReadStream());
                    recordingContent.Headers.ContentType = new MediaTypeHeaderValue(recording.ContentType);
                    content.Add(recordingContent, "recording", recording.FileName);
                }

                if (materials != null)
                {
                    var materialsContent = new StreamContent(materials.OpenReadStream());
                    materialsContent.Headers.ContentType = new MediaTypeHeaderValue(materials.ContentType);
                    content.Add(materialsContent, "materials", materials.FileName);
                }

                var response = await _client.PostAsync($"api/Lecture/{lectureId}/upload-content", content);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LectureViewModel>>();
                
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = apiResponse.Success,
                    Data = apiResponse.Data,
                    Message = apiResponse.Message
                };
            });
        }

        public async Task<byte[]?> DownloadFileAsync(string filePath)
        {
            try
            {
                await AttachAccessTokenAsync();
                var encodedPath = Uri.EscapeDataString(filePath);
                var response = await _client.GetAsync($"api/files/{encodedPath}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

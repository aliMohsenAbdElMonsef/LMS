using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using LMS.MVC.Models.ViewModels.Lecture;
using System.Net.Http.Json;

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
                var lectures = await GetAsync<IEnumerable<LectureViewModel>>($"api/lectures/course/{courseId}");
                return new SuccessServiceResult<IEnumerable<LectureViewModel>>
                {
                    Success = true,
                    Data = lectures ?? Enumerable.Empty<LectureViewModel>()
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> GetLectureByIdAsync(string id)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var lecture = await GetAsync<LectureViewModel>($"api/lectures/{id}");
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = true,
                    Data = lecture
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> CreateLectureAsync(CreateLectureViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var lecture = await PostAsync<LectureViewModel>($"api/lectures/create", JsonContent.Create(model));
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = true,
                    Data = lecture,
                    Message = "Lecture created successfully"
                };
            });
        }

        public async Task<SuccessServiceResult<LectureViewModel>> UpdateLectureAsync(string id, UpdateLectureViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var lecture = await PutAsync<LectureViewModel>($"api/lectures/update/{id}", JsonContent.Create(model));
                return new SuccessServiceResult<LectureViewModel>
                {
                    Success = true,
                    Data = lecture,
                    Message = "Lecture updated successfully"
                };
            });
        }

        public async Task<SuccessServiceResult<bool>> TrackProgressAsync(string lectureId, int watchedSeconds)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var model = new { LectureId = lectureId, WatchedSeconds = watchedSeconds };
                await PostAsync<object>($"api/lectures/track-progress", JsonContent.Create(model));
                return new SuccessServiceResult<bool>
                {
                    Success = true,
                    Data = true
                };
            });
        }

        public async Task<bool> DeleteLectureAsync(string id)
        {
            try
            {
                await DeleteAsync<object>($"api/lectures/delete/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

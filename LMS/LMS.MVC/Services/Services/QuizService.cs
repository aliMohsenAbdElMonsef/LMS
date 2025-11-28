using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using LMS.MVC.Models.ViewModels.Quiz;
using System.Net.Http.Json;

namespace LMS.MVC.Services.Services
{
    internal class QuizService : BaseMVCServices, IQuizService
    {
        public QuizService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
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

        public async Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetQuizzesByCourseAsync(string courseId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<IEnumerable<QuizItemViewModel>>>($"api/quizzes/course/{courseId}");
                return result;
            });
        }

        public async Task<SuccessServiceResult<QuizItemViewModel>> GetQuizByIdAsync(string id)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<QuizItemViewModel>>($"api/quizzes/{id}");
                return result;
            });
        }

        public async Task<SuccessServiceResult<TakeQuizViewModel>> GetQuizForTakingAsync(string id)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<TakeQuizViewModel>>($"api/quizzes/take/{id}");
                return result;
            });
        }

        public async Task<SuccessServiceResult<bool>> StartQuizAsync(string id)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await PostAsync<SuccessServiceResult<bool>>($"api/quizzes/start/{id}", null);
                return result;
            });
        }

        public async Task<SuccessServiceResult<QuizItemViewModel>> CreateQuizAsync(CreateQuizViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await PostAsync<SuccessServiceResult<QuizItemViewModel>>($"api/quizzes/create", JsonContent.Create(model));
                return result;
            });
        }

        public async Task<SuccessServiceResult<QuizItemViewModel>> UpdateQuizAsync(string id, UpdateQuizViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await PutAsync<SuccessServiceResult<QuizItemViewModel>>($"api/quizzes/update/{id}", JsonContent.Create(model));
                return result;
            });
        }

        public async Task<SuccessServiceResult<QuizResultViewModel>> SubmitQuizAsync(SubmitQuizViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await PostAsync<SuccessServiceResult<QuizResultViewModel>>($"api/quizzes/submit", JsonContent.Create(model));
                return result;
            });
        }

        public async Task<SuccessServiceResult<QuizResultViewModel>> GetQuizResultsAsync(string quizId, string username)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<QuizResultViewModel>>($"api/quizzes/results/{quizId}?username={username}");
                return result;
            });
        }

        public async Task<bool> DeleteQuizAsync(string id)
        {
            try
            {
                await DeleteAsync<object>($"api/quizzes/delete/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetQuizzesByInstructorAsync(string instructorId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<IEnumerable<QuizItemViewModel>>>($"api/quizzes/instructor/{instructorId}");
                return result;
            });
        }
    }
}

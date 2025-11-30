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
            try
            {
                Console.WriteLine($"🔄 Updating quiz {id}...");
                Console.WriteLine($"Quiz Data: Title={model.Title}, Questions={model.Questions.Count}");
                
                // Log each question being sent
                for (int i = 0; i < model.Questions.Count; i++)
                {
                    var q = model.Questions[i];
                    var textPreview = string.IsNullOrEmpty(q.Text) ? "" : q.Text.Substring(0, Math.Min(30, q.Text.Length));
                    Console.WriteLine($"  Question {i}: Id={q.Id ?? "NULL"}, Type={q.Type}, Text={textPreview}...");
                }
                
                // API expects PUT /api/quizzes/update with ID in the body, not in the URL
                var response = await _client.PutAsJsonAsync($"api/quizzes/update", model);
                
                Console.WriteLine($"Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ API Error Response: {errorContent}");
                    
                    // Try to parse as JSON to get more details
                    try
                    {
                        var errorJson = System.Text.Json.JsonDocument.Parse(errorContent);
                        Console.WriteLine($"Parsed error: {errorJson.RootElement}");
                    }
                    catch { }
                    
                    return new SuccessServiceResult<QuizItemViewModel>
                    {
                        Success = false,
                        Message = $"API Error ({response.StatusCode}): {errorContent}"
                    };
                }
                
                var result = await response.Content.ReadFromJsonAsync<SuccessServiceResult<QuizItemViewModel>>();
                Console.WriteLine($"✅ Update successful: {result?.Success}");
                
                return result ?? new SuccessServiceResult<QuizItemViewModel>
                {
                    Success = false,
                    Message = "Empty response from API"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in UpdateQuizAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                return new SuccessServiceResult<QuizItemViewModel>
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<SuccessServiceResult<QuizResultViewModel>> SubmitQuizAsync(SubmitQuizViewModel model)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await PostAsync<SuccessServiceResult<QuizResultViewModel>>($"api/quizzes/submit", JsonContent.Create(model));
                return result;
            });
        }

        public async Task<SuccessServiceResult<QuizResultViewModel>> GetQuizResultsAsync(string quizId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<QuizResultViewModel>>($"api/quizzes/results/{quizId}");
                return result;
            });
        }

        public async Task<SuccessServiceResult<LMS.BusinessLogic.DTOs.Quiz.StudentQuizStatusDTO>> GetQuizStatusAsync(string quizId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<LMS.BusinessLogic.DTOs.Quiz.StudentQuizStatusDTO>>($"api/quizzes/status/{quizId}");
                return result;
            });
        }

        public async Task<SuccessServiceResult<IEnumerable<LMS.BusinessLogic.DTOs.Quiz.QuizSubmissionDTO>>> GetQuizSubmissionsAsync(string quizId)
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<IEnumerable<LMS.BusinessLogic.DTOs.Quiz.QuizSubmissionDTO>>>($"api/quizzes/submissions/{quizId}");
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

        public async Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetMyQuizzesAsync()
        {
            return await ExecuteApiCallAsync(async () =>
            {
                var result = await GetAsync<SuccessServiceResult<IEnumerable<QuizItemViewModel>>>("api/quizzes/my-quizzes");
                return result;
            });
        }
    }
}

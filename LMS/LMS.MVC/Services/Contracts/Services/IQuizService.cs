
using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Services.Response;
using LMS.BusinessLogic.DTOs.Quiz; // Added for ManualGradeDTO and QuizSubmissionDTO

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IQuizService
    {
        Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetQuizzesByCourseAsync(string courseId);
        Task<SuccessServiceResult<QuizItemViewModel>> GetQuizByIdAsync(string id);
        Task<SuccessServiceResult<TakeQuizViewModel>> GetQuizForTakingAsync(string id);
        Task<SuccessServiceResult<bool>> StartQuizAsync(string id);
        Task<SuccessServiceResult<QuizItemViewModel>> CreateQuizAsync(CreateQuizViewModel model);
        Task<SuccessServiceResult<QuizItemViewModel>> UpdateQuizAsync(string id, UpdateQuizViewModel model);
        Task<SuccessServiceResult<QuizResultViewModel>> SubmitQuizAsync(SubmitQuizViewModel model);
        Task<SuccessServiceResult<QuizResultViewModel>> GetQuizResultsAsync(string quizId);
        Task<SuccessServiceResult<QuizResultViewModel>> GetStudentQuizResultAsync(string quizId, string studentId);
        Task<SuccessServiceResult<LMS.BusinessLogic.DTOs.Quiz.StudentQuizStatusDTO>> GetQuizStatusAsync(string quizId);
        Task<SuccessServiceResult<IEnumerable<LMS.BusinessLogic.DTOs.Quiz.QuizSubmissionDTO>>> GetQuizSubmissionsAsync(string quizId);
        Task<SuccessServiceResult<bool>> GradeQuizAsync(ManualGradeDTO dto); // Added method
        Task<bool> DeleteQuizAsync(string id);
        Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetQuizzesByInstructorAsync(string instructorId);
        Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetMyQuizzesAsync();
    }
}

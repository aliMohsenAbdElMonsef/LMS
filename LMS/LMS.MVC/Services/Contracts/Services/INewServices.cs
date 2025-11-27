using LMS.MVC.Services.Response;
using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Models.ViewModels.Lecture;
using LMS.MVC.Models.ViewModels.CourseReview;
using LMS.MVC.Models.ViewModels.Certificate;
using LMS.MVC.Models.ViewModels.Notification;

namespace LMS.MVC.Services.Contracts.Services
{
    // Quiz Service Interface
    public interface IQuizService
    {
        Task<SuccessServiceResult<IEnumerable<QuizItemViewModel>>> GetQuizzesByCourseAsync(string courseId);
        Task<SuccessServiceResult<QuizItemViewModel>> GetQuizByIdAsync(string id);
        Task<SuccessServiceResult<TakeQuizViewModel>> GetQuizForTakingAsync(string id);
        Task<SuccessServiceResult<bool>> StartQuizAsync(string id);
        Task<SuccessServiceResult<QuizItemViewModel>> CreateQuizAsync(CreateQuizViewModel model);
        Task<SuccessServiceResult<QuizItemViewModel>> UpdateQuizAsync(string id, UpdateQuizViewModel model);
        Task<SuccessServiceResult<QuizResultViewModel>> SubmitQuizAsync(SubmitQuizViewModel model);
        Task<SuccessServiceResult<QuizResultViewModel>> GetQuizResultsAsync(string quizId, string username);
        Task<bool> DeleteQuizAsync(string id);
    }

    // Lecture Service Interface
    public interface ILectureService
    {
        Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetLecturesByCourseAsync(string courseId);
        Task<SuccessServiceResult<LectureViewModel>> GetLectureByIdAsync(string id);
        Task<SuccessServiceResult<LectureViewModel>> CreateLectureAsync(CreateLectureViewModel model);
        Task<SuccessServiceResult<LectureViewModel>> UpdateLectureAsync(string id, UpdateLectureViewModel model);
        Task<SuccessServiceResult<bool>> TrackProgressAsync(string lectureId, int watchedSeconds);
        Task<bool> DeleteLectureAsync(string id);
    }

    // Profile Service Interface
    public interface IProfileService
    {
        Task<SuccessServiceResult<Models.ViewModels.Profile.ProfileViewModel>> GetProfileAsync(string userId);
        Task<SuccessServiceResult<Models.ViewModels.Profile.ProfileViewModel>> UpdateProfileAsync(string userId, Models.ViewModels.Profile.UpdateProfileViewModel model);
        Task<SuccessServiceResult<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<SuccessServiceResult<Models.ViewModels.Profile.UserStatsViewModel>> GetUserStatsAsync(string userId);
    }

    // Notification Service Interface
    public interface INotificationService
    {
        Task<SuccessServiceResult<IEnumerable<NotificationViewModel>>> GetUserNotificationsAsync(string userId);
        Task<SuccessServiceResult<IEnumerable<NotificationViewModel>>> GetUnreadNotificationsAsync(string userId);
        Task<SuccessServiceResult<int>> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId);
    }

    // Certificate Service Interface
    public interface ICertificateService
    {
        Task<SuccessServiceResult<IEnumerable<CertificateViewModel>>> GetUserCertificatesAsync(string userId);
        Task<SuccessServiceResult<CertificateViewModel>> GetCertificateByIdAsync(string id);
        Task<SuccessServiceResult<byte[]>> DownloadCertificateAsync(string id);
        Task<SuccessServiceResult<string>> GenerateCertificateAsync(string courseId, string userId);
    }

    // Course Review Service Interface
    public interface ICourseReviewService
    {
        Task<SuccessServiceResult<IEnumerable<CourseReviewViewModel>>> GetCourseReviewsAsync(string courseId);
        Task<SuccessServiceResult<CourseReviewViewModel>> CreateReviewAsync(CreateCourseReviewViewModel model);
        Task<SuccessServiceResult<CourseReviewViewModel>> UpdateReviewAsync(string id, CreateCourseReviewViewModel model);
        Task<bool> DeleteReviewAsync(string id);
        Task<SuccessServiceResult<double>> GetCourseRatingAsync(string courseId);
    }
}

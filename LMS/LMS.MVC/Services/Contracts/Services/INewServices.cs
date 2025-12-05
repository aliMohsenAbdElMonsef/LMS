using LMS.MVC.Services.Response;
using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Models.ViewModels.Lecture;
using LMS.MVC.Models.ViewModels.CourseReview;
using LMS.MVC.Models.ViewModels.Certificate;
using LMS.MVC.Models.ViewModels.Notification;
using Microsoft.AspNetCore.Http;

namespace LMS.MVC.Services.Contracts.Services
{

    public interface ILectureService
    {
        Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetLecturesByCourseAsync(string courseId);
        Task<SuccessServiceResult<LectureViewModel>> GetLectureByIdAsync(string id);
        Task<SuccessServiceResult<LectureViewModel>> CreateLectureAsync(CreateLectureViewModel model);
        Task<SuccessServiceResult<LectureViewModel>> UpdateLectureAsync(string id, UpdateLectureViewModel model);
        Task<SuccessServiceResult<bool>> TrackProgressAsync(string lectureId, int watchedSeconds);
        Task<bool> DeleteLectureAsync(string id);
        Task<SuccessServiceResult<bool>> LaunchLectureAsync(string lectureId, string zoomLink);
        Task<SuccessServiceResult<bool>> RescheduleLectureAsync(string lectureId, DateTime newDate, TimeSpan newStartTime);
        Task<SuccessServiceResult<bool>> CheckLectureConflictAsync(string courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, string excludeLectureId = null);
        Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetInstructorLecturesAsync(string instructorId);
        Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetUpcomingLecturesAsync(string courseId);
        Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetTodayLecturesAsync(string courseId);
        Task<SuccessServiceResult<IEnumerable<LectureViewModel>>> GetMyLecturesAsync(string userId, string userRole);
        Task<SuccessServiceResult<AttendanceStatisticsViewModel>> GetAttendanceStatisticsAsync(string userId);
        Task<SuccessServiceResult<AttendanceStatisticsViewModel>> GetCourseAttendanceStatisticsAsync(string courseId);
        Task<SuccessServiceResult<bool>> JoinLectureAsync(string lectureId);
        Task<SuccessServiceResult<LectureViewModel>> UploadLectureContentAsync(string lectureId, IFormFile? recording, IFormFile? materials, string userId, string userRole);
        Task<byte[]?> DownloadFileAsync(string filePath);

    }


    public interface IProfileService
    {
        Task<SuccessServiceResult<Models.ViewModels.Profile.ProfileViewModel>> GetProfileAsync(string userId);
        Task<SuccessServiceResult<Models.ViewModels.Profile.ProfileViewModel>> UpdateProfileAsync(string userId, Models.ViewModels.Profile.UpdateProfileViewModel model);
        Task<SuccessServiceResult<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<SuccessServiceResult<Models.ViewModels.Profile.UserStatsViewModel>> GetUserStatsAsync(string userId);
    }


    public interface INotificationService
    {
        Task<SuccessServiceResult<IEnumerable<NotificationViewModel>>> GetUserNotificationsAsync(string userId);
        Task<SuccessServiceResult<IEnumerable<NotificationViewModel>>> GetUnreadNotificationsAsync(string userId);
        Task<SuccessServiceResult<int>> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId);
    }


    public interface ICertificateService
    {
        Task<SuccessServiceResult<IEnumerable<CertificateViewModel>>> GetUserCertificatesAsync(string userId);
        Task<SuccessServiceResult<CertificateViewModel>> GetCertificateByIdAsync(string id);
        Task<SuccessServiceResult<byte[]>> DownloadCertificateAsync(string id);
        Task<SuccessServiceResult<string>> GenerateCertificateAsync(string courseId, string userId);
    }


    public interface ICourseReviewService
    {
        Task<SuccessServiceResult<IEnumerable<CourseReviewViewModel>>> GetCourseReviewsAsync(string courseId);
        Task<SuccessServiceResult<CourseReviewViewModel>> CreateReviewAsync(CreateCourseReviewViewModel model);
        Task<SuccessServiceResult<CourseReviewViewModel>> UpdateReviewAsync(string id, CreateCourseReviewViewModel model);
        Task<bool> DeleteReviewAsync(string id);
        Task<SuccessServiceResult<double>> GetCourseRatingAsync(string courseId);
    }
}

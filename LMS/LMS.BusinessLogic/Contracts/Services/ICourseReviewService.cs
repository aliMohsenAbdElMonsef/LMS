using LMS.BusinessLogic.DTOs.CourseReview;
using LMS.BusinessLogic.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICourseReviewService
    {
        Task<ServiceResponseDTO<ReadCourseReviewDTO>> CreateReviewAsync(CreateCourseReviewDTO dto);
        Task<ServiceResponseDTO<ReadCourseReviewDTO>> UpdateReviewAsync(UpdateCourseReviewDTO dto);
        Task<ServiceResponseDTO<ReadCourseReviewDTO>> DeleteReviewAsync(string reviewId);
        Task<ServiceResponseDTO<ReadCourseReviewDTO>> GetReviewByIdAsync(string reviewId);
        Task<ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>>> GetCourseReviewsAsync(string courseId);
        Task<ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>>> GetStudentReviewsAsync(string studentId);
        Task<ServiceResponseDTO<CourseRatingStatsDTO>> GetCourseRatingStatsAsync(string courseId);
        Task<BasicResponseDTO> ModerateReviewAsync(string reviewId, bool approve);
    }
}

using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IEnrollIntoCourseServices<TRead, TCreate, TUpdate> : IBaseService<TRead, TCreate, TUpdate> where TRead : class where TUpdate : class where TCreate : class
    {
        #region Enrollment Management
        Task<BasicResponseDTO> EnrollAsync(TCreate dto);
        Task<BasicResponseDTO> UnenrollFromCourseAsync(TCreate dto);
        Task<BasicResponseDTO> ApproveEnrollment(TUpdate dto);
        Task<BasicResponseDTO> DenyEnrollment(TUpdate dto);
        Task<ServiceResponseDTO<bool>> IsEnrolledIn(RequestEnrollIntoCourseDTO dto);
        #endregion

        #region Retrieval Methods
        Task<ServiceResponseDTO<TRead>> GetEnrollmentByIdAsync(TCreate dto);
        Task<ServiceResponseDTO<List<TRead>>> GetEnrollmentsAsync(string userId);
        Task<ServiceResponseDTO<List<TRead>>> GetCourseEnrollmentsAsync(string courseId);
        #endregion

        #region Check Methods
        Task<bool> IsUserEnrolledAsync(TCreate dto);
        Task<int> GetCourseEnrollmentCountAsync(string courseId);
        #endregion

          
    }
 
}





using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Microsoft.Extensions.Configuration.UserSecrets;


namespace LMS.BusinessLogic.Services
{

    internal abstract class BaseEnrollmentService<TEntity, TReadDTO, TUpdateDTO>
        : BaseServices<TEntity, TReadDTO, RequestEnrollIntoCourseDTO, TUpdateDTO>,
          IEnrollIntoCourseServices<TReadDTO, RequestEnrollIntoCourseDTO, TUpdateDTO>
        where TReadDTO : class
        where TUpdateDTO : class
        where TEntity : class
    {
        protected readonly IBaseRepository<TEntity, string> _repo;
        protected readonly IUnitOfWork _unitOfWork;

        protected BaseEnrollmentService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repo = GetRepo();
        }

        #region Abstract Mappings
        protected abstract override TEntity MapToEntity(RequestEnrollIntoCourseDTO dto);
        protected abstract override TEntity UpdateToEntity(TUpdateDTO dto, TEntity existingEntity);
        protected abstract override TReadDTO MapToReadDTO(TEntity entity);
        protected abstract override IBaseRepository<TEntity, string> GetRepo();
        protected abstract override string GetIdFromUpdateDTO(TUpdateDTO dto);
        #endregion

        #region Enrollment Management (Base Implementation)
        public abstract Task<BasicResponseDTO> EnrollAsync(RequestEnrollIntoCourseDTO dto);


        public abstract Task<BasicResponseDTO> UnenrollFromCourseAsync(RequestEnrollIntoCourseDTO dto);

        public abstract Task<BasicResponseDTO> ApproveEnrollment(TUpdateDTO dto);
        public abstract Task<BasicResponseDTO> DenyEnrollment(TUpdateDTO dto);
        #endregion

        #region Retrieval Methods
        public abstract Task<ServiceResponseDTO<TReadDTO>> GetEnrollmentByIdAsync(RequestEnrollIntoCourseDTO dto);
        public abstract Task<ServiceResponseDTO<List<TReadDTO>>> GetEnrollmentsAsync(string userId);
        public abstract Task<ServiceResponseDTO<List<TReadDTO>>> GetCourseEnrollmentsAsync(string courseId);
        #endregion

        #region Check Methods
        public abstract Task<bool> IsUserEnrolledAsync(RequestEnrollIntoCourseDTO dto);
        public abstract Task<int> GetCourseEnrollmentCountAsync(string courseId);

        public abstract Task<ServiceResponseDTO<string>> IsEnrolledIn(RequestEnrollIntoCourseDTO dto);


        #endregion
    }
}

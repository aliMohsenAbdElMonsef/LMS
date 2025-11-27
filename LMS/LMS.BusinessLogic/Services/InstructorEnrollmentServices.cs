using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;
using Domain.Enums;

namespace LMS.BusinessLogic.Services
{
    internal class InstructorEnrollIntoCourseService
        : BaseEnrollmentService<InstructorEnrolltoCourse, ReadEnrollIntoCourseDTO, UpdateEnrollIntoCourseDTO>, IInstructorEnrollIntoCourse
    {
        private readonly IInstructorEnrolltoCourseRepository _instructorEnrollRepo;
        private readonly IUnitOfWork _unitOfWork;

        public InstructorEnrollIntoCourseService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _instructorEnrollRepo = _unitOfWork.InstructorEnrollments;
        }

        #region Repository Access
        protected override IBaseRepository<InstructorEnrolltoCourse, string> GetRepo() => _instructorEnrollRepo;
        #endregion

        #region Mapping
        protected override InstructorEnrolltoCourse MapToEntity(RequestEnrollIntoCourseDTO dto)
        {
            return new InstructorEnrolltoCourse
            {
                InstructorId = dto.UserId,
                CourseId = dto.CourseId,
                Status = ApplicationStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };
        }

        protected override InstructorEnrolltoCourse UpdateToEntity(UpdateEnrollIntoCourseDTO dto, InstructorEnrolltoCourse existingEntity)
        {
            return existingEntity;
        }

        protected override ReadEnrollIntoCourseDTO MapToReadDTO(InstructorEnrolltoCourse entity)
        {
            return new ReadEnrollIntoCourseDTO
            {
                UserId = entity.InstructorId,
                UserName = entity.Instructor?.UserName ?? string.Empty,
                CourseId = entity.CourseId,
                CourseName = entity.Course?.Name ?? string.Empty,
                Status = entity.Status.ToString(),
                CourseCode = entity.Course.CourseCode ?? string.Empty,
                CreatedAt = entity.RequestedAt
            };
        }

        protected override string GetIdFromUpdateDTO(UpdateEnrollIntoCourseDTO dto)
            => $"{dto.UserId}_{dto.CourseId}";
        #endregion

        #region Business Logic
        public override async Task<BasicResponseDTO> EnrollAsync(RequestEnrollIntoCourseDTO dto)
        {
            var user = await _unitOfWork.Users.FindByIdAsync(dto.UserId);
            if (user == null)
                return new BasicResponseDTO { Success = false, Message = "Instructor not found." };

            var course = await _unitOfWork.Courses.FindByIdAsync(dto.CourseId);
            if (course == null)
                return new BasicResponseDTO { Success = false, Message = "Course not found." };

            var existing = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(dto.UserId, dto.CourseId);
            if (existing != null && !existing.IsDeleted)
                return new BasicResponseDTO { Success = false, Message = "Instructor already assigned to this course." };

            if (existing != null)
            {
                existing.IsDeleted = false;
                existing.Status = ApplicationStatus.Pending;
                existing.RequestedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
                return new BasicResponseDTO { Success = true, Message = "Enrollment reactivated successfully." };
            }

            var entity = MapToEntity(dto);
            await _instructorEnrollRepo.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return new BasicResponseDTO { Success = true, Message = "Instructor enrollment request submitted successfully." };
        }

        public override async Task<BasicResponseDTO> UnenrollFromCourseAsync(RequestEnrollIntoCourseDTO dto)
        {
            var enrollment = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(dto.UserId, dto.CourseId);
            if (enrollment == null || enrollment.IsDeleted)
                return new BasicResponseDTO { Success = false, Message = "Enrollment not found or already removed." };

            enrollment.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();

            return new BasicResponseDTO { Success = true, Message = "Instructor unenrolled successfully." };
        }

        public override async Task<BasicResponseDTO> ApproveEnrollment(UpdateEnrollIntoCourseDTO dto)
        {
            var enrollment = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(dto.UserId, dto.CourseId);
            if (enrollment == null)
                return new BasicResponseDTO { Success = false, Message = "Enrollment not found." };

            enrollment.Status = ApplicationStatus.Approved;
            enrollment.ApprovedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return new BasicResponseDTO { Success = true, Message = "Instructor enrollment approved successfully." };
        }

        public override async Task<BasicResponseDTO> DenyEnrollment(UpdateEnrollIntoCourseDTO dto)
        {
            var enrollment = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(dto.UserId, dto.CourseId);
            if (enrollment == null)
                return new BasicResponseDTO { Success = false, Message = "Enrollment not found." };

            enrollment.Status = ApplicationStatus.Rejected;
            enrollment.RejectionReason = "Not specified.";
            await _unitOfWork.SaveChangesAsync();

            return new BasicResponseDTO { Success = true, Message = "Instructor enrollment denied." };
        }

        public override async Task<ServiceResponseDTO<ReadEnrollIntoCourseDTO>> GetEnrollmentByIdAsync(RequestEnrollIntoCourseDTO dto)
        {
            var enrollment = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(dto.UserId, dto.CourseId);
            if (enrollment == null)
                return new ServiceResponseDTO<ReadEnrollIntoCourseDTO>
                {
                    Success = false,
                    Message = "Enrollment not found."
                };

            return new ServiceResponseDTO<ReadEnrollIntoCourseDTO>
            {
                Success = true,
                Data = MapToReadDTO(enrollment)
            };
        }

        public override async Task<ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>> GetEnrollmentsAsync(string userId)
        {
            var enrollments = await _instructorEnrollRepo.GetByInstructorIdAsync(userId);
            var result = enrollments.Select(MapToReadDTO).ToList();

            return new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
            {
                Success = true,
                Data = result
            };
        }

        public override async Task<ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>> GetCourseEnrollmentsAsync(string courseId)
        {
            var enrollments = await _instructorEnrollRepo.GetByCourseIdAsync(courseId);
            var result = enrollments.Select(MapToReadDTO).ToList();

            return new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
            {
                Success = true,
                Data = result
            };
        }

        public override async Task<bool> IsUserEnrolledAsync(RequestEnrollIntoCourseDTO dto)
        {
            var exists = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(dto.UserId, dto.CourseId);
            return exists!=null;
        }

        public override async Task<int> GetCourseEnrollmentCountAsync(string courseId)
        {
            var enrollments = await _instructorEnrollRepo.GetByCourseIdAsync(courseId);
            return enrollments.Count;
        }

        public override async Task<ServiceResponseDTO<string>> IsEnrolledIn(RequestEnrollIntoCourseDTO dto)
        {
            var userId = dto.UserId;
            var courseId = dto.CourseId;
            var enrollment = await _instructorEnrollRepo.GetByInstructorAndCourseAsync(userId, courseId);
            
            string status = enrollment?.Status.ToString() ?? "None";

            return new ServiceResponseDTO<string> 
            { 
                Success = true, 
                Data = status,
                Message = "Enrollment status retrieved."
            };
        }
        #endregion
    }
}

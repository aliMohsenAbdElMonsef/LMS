using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using Domain.Entities.RelationTables;
using Domain.Enums;

namespace LMS.BusinessLogic.Services
{
    internal class StudentEnrollIntoCourseService : BaseEnrollmentService<StudentEnrollIntoCourse, ReadStudentEnrollmentDTO, UpdateStudentEnrollmentDTO>, IStudentEnrollment
    {
        private readonly IStudentEnrollIntoCourseRepository _studentEnrollRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public StudentEnrollIntoCourseService(IUnitOfWork unitOfWork, IEmailService emailService) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _studentEnrollRepo = _unitOfWork.StudentEnrollments;
            _emailService = emailService;
        }

        #region Repository Access
        protected override IBaseRepository<StudentEnrollIntoCourse, string> GetRepo() => _studentEnrollRepo;
        #endregion

        #region Base Class Implementation
        protected override StudentEnrollIntoCourse MapToEntity(RequestEnrollIntoCourseDTO dto)
        {
            return new StudentEnrollIntoCourse
            {
                StudentId = dto.UserId,
                CourseId = dto.CourseId,
                progress = 0,
                Status = ApplicationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        protected override StudentEnrollIntoCourse UpdateToEntity(UpdateStudentEnrollmentDTO dto, StudentEnrollIntoCourse existingEntity)
        {
            existingEntity.progress = dto.Progress;
            return existingEntity;
        }

        protected override ReadStudentEnrollmentDTO MapToReadDTO(StudentEnrollIntoCourse entity)
        {
            return new ReadStudentEnrollmentDTO
            {
                UserId = entity.StudentId,
                UserName = entity.Student?.UserName ?? string.Empty,
                CourseId = entity.CourseId,
                CourseName = entity.Course?.Name ?? string.Empty,
                Status = entity.Status.ToString(),
                CourseCode = entity.Course?.CourseCode ?? string.Empty,
                Progress = entity.progress,
                CreatedAt = entity.CreatedAt
            };
        }

        protected override string GetIdFromUpdateDTO(UpdateStudentEnrollmentDTO dto)
            => $"{dto.UserId}_{dto.CourseId}";

        public override async Task<BasicResponseDTO> EnrollAsync(RequestEnrollIntoCourseDTO dto)
        {
            
            var user = await _unitOfWork.Users.FindByIdAsync(dto.UserId);
            if (user == null)
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };

            var course = await _unitOfWork.Courses.FindByIdAsync(dto.CourseId);
            if (course == null)
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "Course not found."
                };

            var existing = await _studentEnrollRepo.GetEnrollmentIncludingDeletedAsync(dto.UserId, dto.CourseId);
            if (existing != null && !existing.IsDeleted && existing.Status != ApplicationStatus.Rejected)
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "Student is already enrolled in this course."
                };
            if (existing != null)
            {
                existing.IsDeleted = false;
                existing.Status = ApplicationStatus.Pending;
                if (course.EveryStuCouldEnroll)
                {
                    existing.Status = ApplicationStatus.Approved;
                }
                existing.CreatedAt = DateTime.UtcNow;
                
                await _unitOfWork.SaveChangesAsync();

                return new BasicResponseDTO
                {
                    Success = true,
                    Message = "Enrollment created successfully."
                };
            }
            var entity = MapToEntity(dto);
            if (course.EveryStuCouldEnroll)
            {
                entity.Status = ApplicationStatus.Approved;
            }
            await _studentEnrollRepo.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return new BasicResponseDTO
            {
                Success = true,
                Message = "Enrollment created successfully."
            };
            
        }

        public override async Task<BasicResponseDTO> UnenrollFromCourseAsync(RequestEnrollIntoCourseDTO dto)
        {
            var user = await _unitOfWork.Users.FindByIdAsync(dto.UserId);
            if (user == null)
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };

            var course = await _unitOfWork.Courses.FindByIdAsync(dto.CourseId);
            if (course == null)
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "Course not found."
                };

            var enrollment = await _studentEnrollRepo.GetFirstOrDefaultAsync(dto.UserId, dto.CourseId);
            if (enrollment == null || enrollment.IsDeleted)
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "Enrollment not found or already removed."
                };
            await _studentEnrollRepo.DeleteByEntityAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            return new BasicResponseDTO
            {
                Success = true,
                Message = "Student unenrolled successfully."
            };
        }

        #endregion

        #region Interface Implementation

        public override async Task<BasicResponseDTO> ApproveEnrollment(UpdateStudentEnrollmentDTO dto)
        {
            var enrollment = await _studentEnrollRepo.GetFirstOrDefaultAsync(dto.UserId, dto.CourseId, "Student,Course");
            if (enrollment == null)
                return new BasicResponseDTO { Success = false, Message = "Enrollment not found." };

            enrollment.Status = ApplicationStatus.Approved;
            await _unitOfWork.SaveChangesAsync();

            if (enrollment.Student != null && !string.IsNullOrEmpty(enrollment.Student.Email))
            {
                string subject = "Course Enrollment Approved";
                string body = $@"
                    <h3>Hello {enrollment.Student.UserName},</h3>
                    <p>Your enrollment in the course <strong>{enrollment.Course?.Name}</strong> has been <strong>APPROVED</strong>.</p>
                    <p>You can now access the course content.</p>";
                
                _ = _emailService.SendEmailAsync(enrollment.Student.Email, subject, body);
            }

            return new BasicResponseDTO { Success = true, Message = "Enrollment approved successfully." };
        }

        public override async Task<BasicResponseDTO> DenyEnrollment(UpdateStudentEnrollmentDTO dto)
        {
            var enrollment = await _studentEnrollRepo.GetFirstOrDefaultAsync(dto.UserId, dto.CourseId, "Student,Course");
            if (enrollment == null)
                return new BasicResponseDTO { Success = false, Message = "Enrollment not found." };

            enrollment.Status = ApplicationStatus.Rejected;
            await _unitOfWork.SaveChangesAsync();

            if (enrollment.Student != null && !string.IsNullOrEmpty(enrollment.Student.Email))
            {
                string subject = "Course Enrollment Denied";
                string body = $@"
                    <h3>Hello {enrollment.Student.UserName},</h3>
                    <p>Your enrollment in the course <strong>{enrollment.Course?.Name}</strong> has been <strong>DENIED</strong>.</p>
                    <p>Please contact the administrator for more information.</p>";

                _ = _emailService.SendEmailAsync(enrollment.Student.Email, subject, body);
            }

            return new BasicResponseDTO { Success = true, Message = "Enrollment denied." };
        }

        public override async Task<ServiceResponseDTO<ReadStudentEnrollmentDTO>> GetEnrollmentByIdAsync(RequestEnrollIntoCourseDTO dto)
        {
            var enrollment = await _studentEnrollRepo.GetFirstOrDefaultAsync(dto.UserId, dto.CourseId, "Student,Course");
            if (enrollment == null || enrollment.IsDeleted)
                return new ServiceResponseDTO<ReadStudentEnrollmentDTO>
                {
                    Success = false,
                    Message = "Enrollment not found."
                };

            return new ServiceResponseDTO<ReadStudentEnrollmentDTO>
            {
                Success = true,
                Data = MapToReadDTO(enrollment)
            };
        }

        public override async Task<ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>> GetEnrollmentsAsync(string userId)
        {
            var allEnrollments = await _studentEnrollRepo.GetAllAsync();
            var filtered = allEnrollments.Where(e => e.StudentId == userId && !e.IsDeleted).ToList();
            var result = filtered.Select(MapToReadDTO).ToList();

            return new ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>
            {
                Success = true,
                Data = result
            };
        }

        public override async Task<ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>> GetCourseEnrollmentsAsync(string courseId)
        {
            var allEnrollments = await _studentEnrollRepo.GetAllAsync();
            var filtered = allEnrollments.Where(e => e.CourseId == courseId && !e.IsDeleted).ToList();
            var result = filtered.Select(MapToReadDTO).ToList();

            return new ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>
            {
                Success = true,
                Data = result
            };
        }

        public override async Task<bool> IsUserEnrolledAsync(RequestEnrollIntoCourseDTO dto)
        {
            var enrollment = await _studentEnrollRepo.GetFirstOrDefaultAsync(dto.UserId, dto.CourseId);
            return enrollment != null && !enrollment.IsDeleted;
        }

        public override async Task<int> GetCourseEnrollmentCountAsync(string courseId)
        {
            var allEnrollments = await _studentEnrollRepo.GetAllAsync();
            return allEnrollments.Count(e => e.CourseId == courseId && !e.IsDeleted);
        }

        public async Task<ServiceResponseDTO<double>> GetAverageProgressForCourseAsync(string courseId)
        {
            var allEnrollments = await _studentEnrollRepo.GetAllAsync();
            var validEnrollments = allEnrollments
                .Where(e => e.CourseId == courseId && !e.IsDeleted)
                .ToList();

            if (!validEnrollments.Any())
            {
                return new ServiceResponseDTO<double>
                {
                    Success = false,
                    Message = "No active enrollments found for this course.",
                    Data = 0
                };
            }

            double averageProgress = validEnrollments.Average(e => e.progress);

            return new ServiceResponseDTO<double>
            {
                Success = true,
                Message = "Average progress retrieved successfully.",
                Data = Math.Round(averageProgress, 2)
            };
        }

        public Task<ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>> GetStudentsWithLowProgress(string courseId)
        {
            var course = _unitOfWork.Courses.GetByIdWithAcceptedInstructors(courseId);
            var enrollments = course.Students.Where(s => s.progress < course.MinPerformanceScore).ToList();
            var result = enrollments.Select(s => MapToReadDTO(s)).ToList();

            var responseDTO = new ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>
            {
                Success = true,
                Message = "Enrollments found successfully.",
                Data = result
            };

            return Task.FromResult(responseDTO);
        }

        public override async Task<ServiceResponseDTO<string>> IsEnrolledIn(RequestEnrollIntoCourseDTO dto)
        {
            var userId = dto.UserId;
            var courseId = dto.CourseId;
            var enrollment = await _studentEnrollRepo.GetFirstOrDefaultAsync(userId, courseId);
            
            string status = (enrollment != null && !enrollment.IsDeleted) ? enrollment.Status.ToString() : "None";

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

using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using LMS.DataAccess.Contracts;

namespace LMS.BusinessLogic.Services
{
    internal class EnrollmentManagementService: IEnrollmentManagement
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>> GetFilteredEnrollmentsAsync(string? status, string? userSearch, string? courseSearch, string? role)
        {
            var studentQuery = _unitOfWork.StudentEnrollments
                .GetQueryable()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Select(e => new ReadEnrollIntoCourseDTO
                {
                    UserId = e.StudentId,
                    UserName = e.Student.UserName,
                    UserEmail = e.Student.Email,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    Role = "Student",
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt
                });

            var instructorQuery = _unitOfWork.InstructorEnrollments
                .GetQueryable()
                .Include(e => e.Instructor)
                .Include(e => e.Course)
                .Select(e => new ReadEnrollIntoCourseDTO
                {
                    UserId = e.InstructorId,
                    UserName = e.Instructor.UserName,
                    UserEmail = e.Instructor.Email,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    Role = "Instructor",
                    Status = e.Status.ToString(),
                    CreatedAt = e.RequestedAt
                });

            var query = studentQuery.Concat(instructorQuery);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(e => e.Role.ToLower() == role.ToLower());

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrEmpty(userSearch))
                query = query.Where(e =>
                    e.UserName.Contains(userSearch) ||
                    e.UserEmail.Contains(userSearch));

            if (!string.IsNullOrEmpty(courseSearch))
                query = query.Where(e =>
                    e.CourseName.Contains(courseSearch));

            var result = await query
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            return new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
            {
                Success = true,
                Data = result,
                Message = "Enrollments retrieved successfully."
            };
        }
    }
}

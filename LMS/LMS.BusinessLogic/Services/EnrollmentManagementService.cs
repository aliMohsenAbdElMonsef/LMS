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
            try
            {
                var studentQuery = _unitOfWork.StudentEnrollments
                    .GetQueryable()
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .AsNoTracking();

                var instructorQuery = _unitOfWork.InstructorEnrollments
                    .GetQueryable()
                    .Include(e => e.Instructor)
                    .Include(e => e.Course)
                    .AsNoTracking();


                if (!string.IsNullOrEmpty(userSearch))
                {
                    userSearch = userSearch.ToLower();
                    studentQuery = studentQuery.Where(e => e.Student.UserName.ToLower().Contains(userSearch) || e.Student.Email.ToLower().Contains(userSearch));
                    instructorQuery = instructorQuery.Where(e => e.Instructor.UserName.ToLower().Contains(userSearch) || e.Instructor.Email.ToLower().Contains(userSearch));
                }

                if (!string.IsNullOrEmpty(courseSearch))
                {
                    courseSearch = courseSearch.ToLower();
                    studentQuery = studentQuery.Where(e => e.Course.Name.ToLower().Contains(courseSearch) || e.Course.CourseCode.ToLower().Contains(courseSearch));
                    instructorQuery = instructorQuery.Where(e => e.Course.Name.ToLower().Contains(courseSearch) || e.Course.CourseCode.ToLower().Contains(courseSearch));
                }

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.Enums.ApplicationStatus>(status, true, out var statusEnum))
                {
                    studentQuery = studentQuery.Where(e => e.Status == statusEnum);
                    instructorQuery = instructorQuery.Where(e => e.Status == statusEnum);
                }


                var studentEnrollments = await studentQuery.ToListAsync();
                var instructorEnrollments = await instructorQuery.ToListAsync();


                var studentDtos = studentEnrollments.Select(e => new ReadEnrollIntoCourseDTO
                {
                    UserId = e.StudentId,
                    UserName = e.Student.UserName,
                    UserEmail = e.Student.Email,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    CourseCode = e.Course.CourseCode,
                    Role = "Student",
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt
                });

                var instructorDtos = instructorEnrollments.Select(e => new ReadEnrollIntoCourseDTO
                {
                    UserId = e.InstructorId,
                    UserName = e.Instructor.UserName,
                    UserEmail = e.Instructor.Email,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    CourseCode = e.Course.CourseCode,
                    Role = "Instructor",
                    Status = e.Status.ToString(),
                    CreatedAt = e.RequestedAt
                });

                var allEnrollments = new List<ReadEnrollIntoCourseDTO>();

                if (string.IsNullOrEmpty(role) || role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    allEnrollments.AddRange(studentDtos);
                }

                if (string.IsNullOrEmpty(role) || role.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
                {
                    allEnrollments.AddRange(instructorDtos);
                }


                var result = allEnrollments.OrderByDescending(e => e.CreatedAt).ToList();

                return new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Data = result,
                    Message = "Enrollments retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving enrollments: {ex.Message}",
                    Data = new List<ReadEnrollIntoCourseDTO>()
                };
            }
        }
    }
}

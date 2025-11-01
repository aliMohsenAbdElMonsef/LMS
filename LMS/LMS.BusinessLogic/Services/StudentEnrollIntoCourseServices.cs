using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class StudentEnrollIntoCourseServices : BaseServices<StudentEnrollIntoCourse,
        ReadStudentEnrollIntoCourseDTO, CreateStudentEnrollIntoCourseDTO,
        UpdateStudentEnrollIntoCourseDTO>, IStudentEnrollIntoCourseServices
    {
        public StudentEnrollIntoCourseServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        protected override StudentEnrollIntoCourse MapToEntity(CreateStudentEnrollIntoCourseDTO dto)
        {
            StudentEnrollIntoCourse StudentEnrollIntoCourse = new StudentEnrollIntoCourse
            {
                CourseId = dto.CourseId,
                StudentId = dto.StudentId
            };
            return StudentEnrollIntoCourse;
        }

        protected override StudentEnrollIntoCourse UpdateToEntity(UpdateStudentEnrollIntoCourseDTO dto, StudentEnrollIntoCourse existingEntity)
        {
            existingEntity.CourseId = dto.CourseId ?? existingEntity.CourseId;
            existingEntity.StudentId = dto.StudentId ?? existingEntity.StudentId;
            existingEntity.progress = dto.Progress;

            return existingEntity;
        }

        protected override ReadStudentEnrollIntoCourseDTO MapToReadDTO(StudentEnrollIntoCourse entity)
        {
            ReadStudentEnrollIntoCourseDTO readStudentEnrolltoCourseDTO = new ReadStudentEnrollIntoCourseDTO
            {
                StudentId = entity.StudentId,
                CourseId = entity.CourseId,
                StudentName = entity.Student.FirstName + " " + entity.Student.LastName,
                CourseName = entity.Course.Name,
                Progress = entity.progress
            };
            return readStudentEnrolltoCourseDTO;
        }

        protected override IBaseRepository<StudentEnrollIntoCourse, string> GetRepo() => _unitOfWork.StudentEnrollments;

       

        #region Enrollment Management

        public async Task<BasicResponseDTO> EnrollStudentAsync(CreateStudentEnrollIntoCourseDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.StudentId) || string.IsNullOrWhiteSpace(dto.CourseId))
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Student ID and Course ID are required.",
                        Errors = new List<string> { "Invalid input data." }
                    };
                }

                var existingEnrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId );

                if (existingEnrollment != null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Student is already enrolled in this course.",
                        Errors = new List<string> { "Duplicate enrollment detected." }
                    };
                }

                // Check if student exists
                var student = await _unitOfWork.Users.FindByIdAsync(dto.StudentId);
                if (student == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Student not found.",
                        Errors = new List<string> { $"Student with ID {dto.StudentId} does not exist." }
                    };
                }

                // Check if course exists
                var course = await _unitOfWork.Courses.FindByIdAsync(dto.CourseId);
                if (course == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Course not found.",
                        Errors = new List<string> { $"Course with ID {dto.CourseId} does not exist." }
                    };
                }

                // Create enrollment
                var enrollment = new StudentEnrollIntoCourse
                {
                    StudentId = dto.StudentId,
                    CourseId = dto.CourseId,
                    //EnrollmentDate = DateTime.UtcNow,
                    progress = 0.0,
                    
                };

                await _unitOfWork.StudentEnrollments.CreateAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();

                return new BasicResponseDTO
                {
                    Success = true,
                    Message = "Student enrolled successfully."
                };
            }
            catch (Exception ex)
            {
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while enrolling the student.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        

        public async Task<BasicResponseDTO> UnenrollStudentFromCourseAsync(StudentUnenrollfromCourseDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.StudentId) || string.IsNullOrWhiteSpace(dto.CourseId))
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Student ID and Course ID are required.",
                        Errors = new List<string> { "Invalid input data." }
                    };
                }

                var enrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId );

                if (enrollment == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Active enrollment not found.",
                        Errors = new List<string>
                        {
                            $"No active enrollment found for Student ID {dto.StudentId} in Course ID {dto.CourseId}."
                        }
                    };
                }

                await _unitOfWork.StudentEnrollments.DeleteByEntityAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();

                return new BasicResponseDTO
                {
                    Success = true,
                    Message = "Student unenrolled successfully."
                };
            }
            catch (Exception ex)
            {
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while unenrolling the student.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        #endregion

        
        #region Retrieval Methods

        public async Task<ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>> GetEnrollmentByIdAsync(GetEnrollmentDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.studentId)||string.IsNullOrEmpty(dto.courseId))
                {
                    return new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = false,
                        Message = "Enrollment ID is required.",
                        Errors = new List<string> { "Invalid enrollment ID." }
                    };
                }

                var enrollment = await _unitOfWork.StudentEnrollments
                                .GetFirstOrDefaultAsync(dto.studentId, dto.courseId,
                                  includeProperties: "Student,Course");
                if (enrollment == null)
                {
                    return new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = false,
                        Message = "Enrollment not found.",
                        Errors = new List<string> { $"Student with ID {dto.studentId} doesn't enroll into course with id {dto.courseId}" }
                    };
                }

                var data = new ReadStudentEnrollIntoCourseDTO
                {
                    StudentId = enrollment.StudentId,
                    StudentName = enrollment.Student?.FirstName + " " + enrollment.Student?.LastName ?? "N/A",
                    CourseId = enrollment.CourseId,
                    CourseName = enrollment.Course?.Name ?? "N/A",
                    Progress = enrollment.progress
                };

                return new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                {
                    Success = true,
                    Message = "Enrollment retrieved successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                {
                    Success = false,
                    Message = "An error occurred while retrieving enrollment.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentEnrollmentsAsync(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                    {
                        Success = false,
                        Message = "Student ID is required.",
                        Errors = new List<string> { "Invalid student ID." }
                    };
                }

                var enrollments = await _unitOfWork.StudentEnrollments
                    .GetAllAsync(e => e.StudentId == studentId && e.Course.IsDeleted==false);

                var data = enrollments.Select(e => new ReadStudentEnrollIntoCourseDTO
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FirstName + " " + e.Student?.LastName ?? "N/A",
                    CourseId = e.CourseId,
                    CourseName = e.Course?.Name ?? "N/A",
                    Progress = e.progress
                }).ToList();

                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Message = $"Found {data.Count} enrollments.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving student enrollments.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetCourseEnrollmentsAsync(string courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                    {
                        Success = false,
                        Message = "Course ID is required.",
                        Errors = new List<string> { "Invalid course ID." }
                    };
                }

                var enrollments = await _unitOfWork.StudentEnrollments
                    .GetAllAsync(e => e.CourseId == courseId && e.Student.IsDeleted == false);

                var data = enrollments.Select(e => new ReadStudentEnrollIntoCourseDTO
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FirstName + " " + e.Student?.LastName ?? "N/A",
                    CourseId = e.CourseId,
                    CourseName = e.Course?.Name ?? "N/A",
                    Progress = e.progress
                }).ToList();

                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Message = $"Found {data.Count} students enrolled.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving course enrollments.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Check Methods

        public async Task<bool> IsStudentEnrolledAsync(GetEnrollmentDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.studentId) || string.IsNullOrWhiteSpace(dto.courseId))
                    return false;

                var enrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == dto.studentId && e.CourseId == dto.courseId);

                return enrollment != null;
            }
            catch
            {
                return false;
            }
        }
        public async Task<int> GetCourseEnrollmentCountAsync(string courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(courseId))
                    return 0;

                var enrollments = await _unitOfWork.StudentEnrollments
                    .GetAllAsync(e => e.CourseId == courseId && e.Student.IsDeleted == false);

                return enrollments.Count();
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Statistics
        public async Task<ServiceResponseDTO<double>> GetAverageProgressForCourseAsync(string courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Course ID is required.",
                        Errors = new List<string> { "Invalid course ID." }
                    };
                }

                var enrollments = await _unitOfWork.StudentEnrollments
                    .GetAllAsync(e => e.CourseId == courseId && e.Student.IsDeleted == false);

                if (!enrollments.Any())
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = true,
                        Message = "No active enrollments found for this course.",
                        Data = 0.0
                    };
                }

                double averageProgress = enrollments.Average(e => e.progress);

                return new ServiceResponseDTO<double>
                {
                    Success = true,
                    Message = $"Average progress: {averageProgress:F2}%",
                    Data = averageProgress
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<double>
                {
                    Success = false,
                    Message = "An error occurred while calculating average progress.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentsWithLowProgressAsync(GetStudentLessThersholdDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.courseId))
                {
                    return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                    {
                        Success = false,
                        Message = "Course ID is required.",
                        Errors = new List<string> { "Invalid course ID." }
                    };
                }

                if (dto.threshold < 0 || dto.threshold > 100)
                {
                    return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                    {
                        Success = false,
                        Message = "Threshold must be between 0 and 100.",
                        Errors = new List<string> { "Invalid threshold value." }
                    };
                }

                var enrollments = await _unitOfWork.StudentEnrollments
                    .GetAllAsync(e => e.CourseId == dto.courseId && e.Student.IsDeleted == false && e.progress < dto.threshold);

                var data = enrollments.Select(e => new ReadStudentEnrollIntoCourseDTO
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FirstName + " " + e.Student?.LastName ?? "N/A",
                    CourseId = e.CourseId,
                    CourseName = e.Course?.Name ?? "N/A",
                    Progress = e.progress
                }).ToList();

                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Message = $"Found {data.Count} students with progress below {dto.threshold}%.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving students with low progress.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        protected override string GetIdFromUpdateDTO(UpdateStudentEnrollIntoCourseDTO dto)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
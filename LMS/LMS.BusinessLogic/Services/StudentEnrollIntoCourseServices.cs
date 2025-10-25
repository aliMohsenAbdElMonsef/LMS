using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAcess.Contracts;
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
                Id = Guid.NewGuid().ToString(),
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
                Id = entity.Id,
                StudentName = entity.Student.FirstName + " " + entity.Student.LastName,
                CourseName = entity.Course.Name,
                Progress = entity.progress
            };
            return readStudentEnrolltoCourseDTO;
        }

        protected override IBaseRepository<StudentEnrollIntoCourse, string> GetRepo() => _unitOfWork.StudentEnrollments;

        protected override string GetIdFromUpdateDTO(UpdateStudentEnrollIntoCourseDTO dto) => dto.Id;


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
                var course = await _unitOfWork.Coures.FindByIdAsync(dto.CourseId);
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
                    Id = Guid.NewGuid().ToString(),
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

        public async Task<BasicResponseDTO> UnenrollStudentAsync(string enrollmentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(enrollmentId))
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Enrollment ID is required.",
                        Errors = new List<string> { "Invalid enrollment ID." }
                    };
                }

                var enrollment = await _unitOfWork.StudentEnrollments.FindByIdAsync(enrollmentId);
                if (enrollment == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Enrollment not found.",
                        Errors = new List<string> { $"Enrollment with ID {enrollmentId} does not exist." }
                    };
                }

                await _unitOfWork.StudentEnrollments.UpdateAsync(enrollment);
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

        public async Task<BasicResponseDTO> UnenrollStudentFromCourseAsync(string studentId, string courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId))
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Student ID and Course ID are required.",
                        Errors = new List<string> { "Invalid input data." }
                    };
                }

                var enrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId );

                if (enrollment == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Active enrollment not found.",
                        Errors = new List<string>
                        {
                            $"No active enrollment found for Student ID {studentId} in Course ID {courseId}."
                        }
                    };
                }

                await _unitOfWork.StudentEnrollments.UpdateAsync(enrollment);
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

        #region Progress Management

        public async Task<ServiceResponseDTO<double>> UpdateProgressAfterAssignmentAsync(string studentId, string courseId, double assignmentScore, double assignmentWeight)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId))
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Student ID and Course ID are required.",
                        Errors = new List<string> { "Invalid input data." }
                    };
                }

                if (assignmentScore < 0 || assignmentScore > 100)
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Assignment score must be between 0 and 100.",
                        Errors = new List<string> { "Invalid assignment score." }
                    };
                }

                if (assignmentWeight <= 0 || assignmentWeight > 100)
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Assignment weight must be between 0 and 100.",
                        Errors = new List<string> { "Invalid assignment weight." }
                    };
                }

                // Get current enrollment
                var enrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId );

                if (enrollment == null)
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Active enrollment not found.",
                        Errors = new List<string> { $"No active enrollment found for student {studentId} in course {courseId}." }
                    };
                }

                // SIMPLE CALCULATION: Assignment contribution based on score and weight
                double assignmentContribution = (assignmentScore / 100.0) * assignmentWeight;

                // Add to current progress (simple approach)
                double newProgress = enrollment.progress + assignmentContribution;
                newProgress = Math.Min(newProgress, 100.0); // Cap at 100%

                // Update enrollment
                enrollment.progress = newProgress;
                await _unitOfWork.StudentEnrollments.UpdateAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<double>
                {
                    Success = true,
                    Message = $"Progress updated to {newProgress:F2}% after assignment (Score: {assignmentScore}%, Weight: {assignmentWeight}%).",
                    Data = newProgress
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<double>
                {
                    Success = false,
                    Message = "An error occurred while updating progress after assignment.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<ServiceResponseDTO<double>> UpdateProgressAfterQuizAsync(string studentId, string courseId, double quizScore, int correctAnswers, int totalQuestions)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId))
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Student ID and Course ID are required.",
                        Errors = new List<string> { "Invalid input data." }
                    };
                }

                if (quizScore < 0 || quizScore > 100)
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Quiz score must be between 0 and 100.",
                        Errors = new List<string> { "Invalid quiz score." }
                    };
                }

                // Get current enrollment
                var enrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId );

                if (enrollment == null)
                {
                    return new ServiceResponseDTO<double>
                    {
                        Success = false,
                        Message = "Active enrollment not found.",
                        Errors = new List<string> { $"No active enrollment found for student {studentId} in course {courseId}." }
                    };
                }

                // Quiz progress calculation - fixed small weight
                double quizWeight = 5.0; // Quizzes have smaller impact
                double quizContribution = (quizScore / 100.0) * quizWeight;

                // Add to current progress
                double newProgress = enrollment.progress + quizContribution;
                newProgress = Math.Min(newProgress, 100.0); // Cap at 100%

                // Update enrollment
                enrollment.progress = newProgress;
                await _unitOfWork.StudentEnrollments.UpdateAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<double>
                {
                    Success = true,
                    Message = $"Progress updated to {newProgress:F2}% after quiz (Score: {quizScore}%, {correctAnswers}/{totalQuestions} correct).",
                    Data = newProgress
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<double>
                {
                    Success = false,
                    Message = "An error occurred while updating progress after quiz.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }       
        #endregion
        #region Retrieval Methods

        public async Task<ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>> GetEnrollmentByIdAsync(string enrollmentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(enrollmentId))
                {
                    return new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = false,
                        Message = "Enrollment ID is required.",
                        Errors = new List<string> { "Invalid enrollment ID." }
                    };
                }

                var enrollment = await _unitOfWork.StudentEnrollments.FindByIdAsync(enrollmentId);
                if (enrollment == null)
                {
                    return new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = false,
                        Message = "Enrollment not found.",
                        Errors = new List<string> { $"Enrollment with ID {enrollmentId} does not exist." }
                    };
                }

                var data = new ReadStudentEnrollIntoCourseDTO
                {
                    Id = enrollment.Id,
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
                    .GetAllAsync(e => e.StudentId == studentId );

                var data = enrollments.Select(e => new ReadStudentEnrollIntoCourseDTO
                {
                    Id = e.Id,
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
                    .GetAllAsync(e => e.CourseId == courseId );

                var data = enrollments.Select(e => new ReadStudentEnrollIntoCourseDTO
                {
                    Id = e.Id,
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

        public async Task<bool> IsStudentEnrolledAsync(string studentId, string courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId))
                    return false;

                var enrollment = await _unitOfWork.StudentEnrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

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
                    .GetAllAsync(e => e.CourseId == courseId );

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
                    .GetAllAsync(e => e.CourseId == courseId );

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
        public async Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentsWithLowProgressAsync(string courseId, double threshold = 30.0)
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

                if (threshold < 0 || threshold > 100)
                {
                    return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                    {
                        Success = false,
                        Message = "Threshold must be between 0 and 100.",
                        Errors = new List<string> { "Invalid threshold value." }
                    };
                }

                var enrollments = await _unitOfWork.StudentEnrollments
                    .GetAllAsync(e => e.CourseId == courseId  && e.progress < threshold);

                var data = enrollments.Select(e => new ReadStudentEnrollIntoCourseDTO
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FirstName + " " + e.Student?.LastName ?? "N/A",
                    CourseId = e.CourseId,
                    CourseName = e.Course?.Name ?? "N/A",
                    Progress = e.progress
                }).ToList();

                return new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Message = $"Found {data.Count} students with progress below {threshold}%.",
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
        #endregion
    }
}
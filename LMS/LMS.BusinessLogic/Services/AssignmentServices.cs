using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using LMS.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class AssignmentServices : BaseServices<Assignment, ReadAssignmentDTO, CreateAssignmentDTO, UpdateAssignmentDTO>, IAssignmentServices
    {
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public AssignmentServices(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService) : base(unitOfWork)
        {
            _mapper = mapper;
            _emailService = emailService;
        }


        public async Task<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>> GetStudentSubmissionsAsync(string studentId)
        {
            try
            {
                var submissions = await _unitOfWork.Assignments.GetStudentSubmissionsAsync(studentId);
                var dtos = _mapper.Map<List<StudentAssignmentDTO>>(submissions);

                return new ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>
                {
                    Success = true,
                    Message = "Student submissions retrieved successfully",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving submissions: {ex.Message}"
                };
            }
        }
        public async Task<ServiceResponseDTO<IEnumerable<StudentAllAssignmentsDTO>>> GetStudentAllAssignmentsAsync(string studentId)
        {
            try
            {
                // Get all assignments from all courses
                var allCourseAssignments = await _unitOfWork.Assignments.GetAllAsync();
                var allAssignments = new List<StudentAllAssignmentsDTO>();

                foreach (var assignment in allCourseAssignments)
                {
                    // Check if student is enrolled in this course
                    var isEnrolled = await _unitOfWork.StudentEnrollments
                        .IsStudentEnrolledInCourseAsync(studentId, assignment.CourseId);

                    if (!isEnrolled)
                        continue;

                    // Get course info
                    var course = await _unitOfWork.Courses.FindByIdAsync(assignment.CourseId);

                    // Get submission if exists
                    var submission = await _unitOfWork.Assignments
                        .GetStudentAssignmentAsync(assignment.Id, studentId);

                    allAssignments.Add(new StudentAllAssignmentsDTO
                    {
                        AssignmentId = assignment.Id,
                        AssignmentTitle = assignment.Title,
                        CourseName = course?.Name ?? "Unknown",
                        DueDate = assignment.DueDate,
                        IsSubmitted = submission != null,
                        Status = submission?.Status.ToString(),
                        StatusDisplay = submission != null ? GetStatusDisplay(submission.Status) : "Not Submitted",
                        Grade = submission?.Grade,
                        FilePath = submission?.FilePath,
                        SubmissionId = submission?.Id,
                        SubmittedAt = submission?.SubmittedAt
                    });
                }

                return new ServiceResponseDTO<IEnumerable<StudentAllAssignmentsDTO>>
                {
                    Success = true,
                    Message = "Assignments retrieved successfully",
                    Data = allAssignments
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<StudentAllAssignmentsDTO>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private string GetStatusDisplay(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.NotSubmitted => "Not Submitted",
                AssignmentStatus.PendingGrading => "Pending Grading",
                AssignmentStatus.Graded => "Graded",
                _ => "Unknown"
            };
        }
        public async Task<ServiceResponseDTO<AssignmentDetailsDTO>> GetAssignmentWithDetailsAsync(string id)
        {
            var assignment = await _unitOfWork.Assignments.FindByIdAsync(id);
            if (assignment == null)
            {
                return new ServiceResponseDTO<AssignmentDetailsDTO>
                {
                    Success = false,
                    Message = "Assignment not found."
                };
            }

            var submissions = await _unitOfWork.Assignments.GetAssignmentSubmissionsAsync(id);
            var course = await _unitOfWork.Courses.FindByIdAsync(assignment.CourseId);

            var dto = new AssignmentDetailsDTO
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                FilePath = assignment.FilePath,
                UploadDate = assignment.UploadDate,
                DueDate = assignment.DueDate,
                CourseId = assignment.CourseId,
                CourseName = course?.Name ?? "Unknown Course",
                InstructorId = assignment.InstructorId,
                InstructorName = assignment.Instructor?.UserName ?? "Unknown Instructor",
                SubmissionsCount = submissions.Count(),
                StudentSubmissions = _mapper.Map<List<StudentAssignmentDTO>>(submissions)
            };

            return new ServiceResponseDTO<AssignmentDetailsDTO>
            {
                Success = true,
                Message = "Assignment retrieved successfully",
                Data = dto
            };
        }
        public async Task<ServiceResponseDTO<StudentAssignmentDTO>> GetStudentAssignmentByIdAsync(string studentAssignmentId)
        {
            var submission = await _unitOfWork.Assignments.GetStudentAssignmentByIdAsync(studentAssignmentId);
            if (submission == null)
            {
                return new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "Submission not found."
                };
            }

            var dto = _mapper.Map<StudentAssignmentDTO>(submission);
            return new ServiceResponseDTO<StudentAssignmentDTO>
            {
                Success = true,
                Message = "Submission retrieved successfully",
                Data = dto
            };
        }
        public async Task<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>> GetAssignmentsByCourseAsync(string courseId)
        {
            var assignments = await _unitOfWork.Assignments.GetAssignmentsByCourseAsync(courseId);
            var dtos = _mapper.Map<List<ReadAssignmentDTO>>(assignments);

            return new ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>
            {
                Success = true,
                Message = "Assignments retrieved successfully",
                Data = dtos
            };
        }

        public async Task<ServiceResponseDTO<StudentAssignmentDTO>> GetStudentAssignmentAsync(string assignmentId, string studentId)
        {
            var submission = await _unitOfWork.Assignments.GetStudentAssignmentAsync(assignmentId, studentId);
            if (submission == null)
            {
                return new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "Submission not found."
                };
            }

            var dto = _mapper.Map<StudentAssignmentDTO>(submission);
            return new ServiceResponseDTO<StudentAssignmentDTO>
            {
                Success = true,
                Message = "Submission retrieved successfully",
                Data = dto
            };
        }
        public async Task<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>> GetAssignmentSubmissionsAsync(string assignmentId)
        {
            var submissions = await _unitOfWork.Assignments.GetAssignmentSubmissionsAsync(assignmentId);
            var dtos = _mapper.Map<List<StudentAssignmentDTO>>(submissions);

            return new ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>
            {
                Success = true,
                Message = "Submissions retrieved successfully",
                Data = dtos
            };
        }
        public async Task<ServiceResponseDTO<StudentAssignmentDTO>> SubmitAssignmentAsync(SubmitAssignmentDTO submission)
        {
            var assignment = await _unitOfWork.Assignments.FindByIdAsync(submission.AssignmentId);
            if (assignment == null)
            {
                return new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "Assignment not found."
                };
            }

            // Check if the assignment deadline has passed
            if (assignment.DueDate < DateTime.UtcNow)
            {
                return new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "This assignment is past its due date and can no longer be submitted."
                };
            }

            var student = await _unitOfWork.Users.FindByIdAsync(submission.StudentId);
            if (student == null)
            {
                return new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "Student not found."
                };
            }

            // Check if already submitted
            var existingSubmission = await _unitOfWork.Assignments.GetStudentAssignmentAsync(submission.AssignmentId, submission.StudentId);

            if (existingSubmission != null)
            {
                // Update existing submission
                existingSubmission.FilePath = submission.FilePath;
                existingSubmission.SubmittedAt = DateTime.UtcNow;
                existingSubmission.Status = AssignmentStatus.PendingGrading; // NEW: Update status
                existingSubmission.Grade = null; // Reset grade if resubmitting
                existingSubmission.GradedAt = null;

                await _unitOfWork.Assignments.UpdateStudentAssignmentAsync(existingSubmission);
            }
            else
            {
                // Create new submission
                var studentAssignment = new StudentAssignment
                {
                    Id = Guid.NewGuid().ToString(),
                    StudentId = submission.StudentId,
                    AssignmentId = submission.AssignmentId,
                    FilePath = submission.FilePath,
                    SubmittedAt = DateTime.UtcNow,
                    Status = AssignmentStatus.PendingGrading, // NEW: Set initial status
                    Grade = null,
                    GradedAt = null
                };
                await _unitOfWork.Assignments.CreateStudentAssignmentAsync(studentAssignment);
            }

            await _unitOfWork.SaveChangesAsync();

            var updatedSubmission = await _unitOfWork.Assignments.GetStudentAssignmentAsync(submission.AssignmentId, submission.StudentId);
            var resultDto = _mapper.Map<StudentAssignmentDTO>(updatedSubmission);

            return new ServiceResponseDTO<StudentAssignmentDTO>
            {
                Success = true,
                Message = "Assignment submitted successfully",
                Data = resultDto
            };
        }
        public async Task<ServiceResponseDTO<StudentAssignmentDTO>> GradeAssignmentAsync(GradeAssignmentDTO grade)
        {
            var studentAssignment = await _unitOfWork.Assignments.GetStudentAssignmentByIdAsync(grade.StudentAssignmentId);
            if (studentAssignment == null)
            {
                return new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "Submission not found."
                };
            }

            studentAssignment.Grade = grade.Grade;
            studentAssignment.GradedAt = DateTime.UtcNow;
            studentAssignment.Status = AssignmentStatus.Graded;
            studentAssignment.Feedback = grade.Feedback;

            await _unitOfWork.Assignments.UpdateStudentAssignmentAsync(studentAssignment);
            await _unitOfWork.SaveChangesAsync();

            // Send email notification to student
            if (studentAssignment.Student != null && !string.IsNullOrEmpty(studentAssignment.Student.Email))
            {
                var subject = $"Assignment Graded: {studentAssignment.Assignment?.Title}";
                var body = $@"
                    <h1>Assignment Graded</h1>
                    <p>Your assignment <strong>{studentAssignment.Assignment?.Title}</strong> has been graded.</p>
                    <p><strong>Grade:</strong> {grade.Grade}</p>
                    <p><strong>Feedback:</strong> {grade.Feedback}</p>
                    <p>Please log in to the LMS to view more details.</p>";

                try
                {
                    await _emailService.SendEmailAsync(studentAssignment.Student.Email, subject, body);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                    Console.WriteLine($"Failed to send grade notification email: {ex.Message}");
                }
            }

            var resultDto = _mapper.Map<StudentAssignmentDTO>(studentAssignment);

            return new ServiceResponseDTO<StudentAssignmentDTO>
            {
                Success = true,
                Message = "Assignment graded successfully",
                Data = resultDto
            };
        }
        public async Task<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>> GetSubmissionsByStatusAsync(string assignmentId, AssignmentStatus status)
        {
            var submissions = await _unitOfWork.Assignments.GetSubmissionsByStatusAsync(assignmentId, status);
            var dtos = _mapper.Map<List<StudentAssignmentDTO>>(submissions);

            return new ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>
            {
                Success = true,
                Message = $"Submissions with status {status} retrieved successfully",
                Data = dtos
            };
        }
        protected override string GetIdFromUpdateDTO(UpdateAssignmentDTO dto) => dto.Id;

        protected override IBaseRepository<Assignment, string> GetRepo() => _unitOfWork.Assignments;

        protected override Assignment MapToEntity(CreateAssignmentDTO dto)
        {
            return new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                FilePath = dto.FilePath,
                UploadDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                CourseId = dto.CourseId,
                InstructorId = dto.InstructorId
            };
        }

        protected override ReadAssignmentDTO MapToReadDTO(Assignment entity)
        {
            var submissionsCount = _unitOfWork.Assignments.GetAssignmentSubmissionsCountAsync(entity.Id).Result;

            return new ReadAssignmentDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                FilePath = entity.FilePath,
                UploadDate = entity.UploadDate,
                DueDate = entity.DueDate,
                CourseId = entity.CourseId,
                CourseName = entity.Course?.Name ?? "Unknown Course",
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor?.UserName ?? "Unknown Instructor",
                SubmissionsCount = submissionsCount
            };
        }

        protected override Assignment UpdateToEntity(UpdateAssignmentDTO dto, Assignment existingEntity)
        {
            existingEntity.Title = dto.Title ?? existingEntity.Title;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.FilePath = dto.FilePath ?? existingEntity.FilePath;
            existingEntity.DueDate = dto.DueDate ?? existingEntity.DueDate;

            return existingEntity;
        }
        public override async Task<ServiceResponseDTO<ReadAssignmentDTO>> DeleteAsync(string id)
        {
            try
            {
                var assignment = await _unitOfWork.Assignments.FindByIdAsync(id);
                if (assignment == null)
                {
                    return new ServiceResponseDTO<ReadAssignmentDTO>
                    {
                        Success = false,
                        Message = "Assignment not found."
                    };
                }

                // Check if assignment can be deleted (due date not passed or admin)
                // Note: You might want to get current user from context in a real scenario
                if (assignment.DueDate < DateTime.UtcNow)
                {
                    // For instructors, they can only delete before due date
                    // Admins can delete anytime - you would check user roles here
                    // For now, we'll allow deletion but you can add role checking later
                    Console.WriteLine($"Warning: Deleting assignment after due date: {assignment.Title}");
                }

                // Use the base delete functionality which handles soft delete
                return await base.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<ReadAssignmentDTO>
                {
                    Success = false,
                    Message = $"Error deleting assignment: {ex.Message}"
                };
            }
        }

        // ADD THIS METHOD - for the bool return type used by MVC
        public async Task<ServiceResponseDTO<bool>> DeleteAssignmentAsync(string id)
        {
            var result = await base.DeleteAsync(id);
            return new ServiceResponseDTO<bool>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success
            };
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>> GetAssignmentsByInstructorAsync(string instructorId)
        {
            try
            {
                var assignments = await _unitOfWork.Assignments.GetAssignmentsByInstructorAsync(instructorId);
                
                // Filter to only include assignments from courses where instructor has active enrollment
                var instructorEnrollments = await _unitOfWork.InstructorEnrollments.GetAllAsync();
                var activeCourseIds = instructorEnrollments
                    .Where(e => e.InstructorId == instructorId && !e.IsDeleted && e.Status == Domain.Enums.ApplicationStatus.Approved)
                    .Select(e => e.CourseId)
                    .ToHashSet();
                
                var filteredAssignments = assignments.Where(a => activeCourseIds.Contains(a.CourseId)).ToList();
                var dtos = _mapper.Map<List<ReadAssignmentDTO>>(filteredAssignments);

                return new ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>
                {
                    Success = true,
                    Message = "Assignments retrieved successfully",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving assignments: {ex.Message}"
                };
            }
        }

    }
}

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

        public AssignmentServices(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
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


    }
}



using Domain.Enums;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.Entity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;
        private readonly IFileUploadService _fileUploadService;

        public AssignmentController(IUnitOfServices unitOfServices, IFileUploadService fileUploadService)
        {
            _unitOfServices = unitOfServices;
            _fileUploadService = fileUploadService;
        }

        private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        private IAssignmentServices AssignmentService => _unitOfServices.Assignments;

        [HttpPost("create-with-file")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadAssignmentDTO>>> CreateAssignmentWithFile(
            [FromForm] CreateAssignmentDTO assignment,
            [FromForm] IFormFile? assignmentFile)
        {
            try
            {
                var instructorId = GetCurrentUserId() ?? "";

                assignment.InstructorId = instructorId;
                assignment.FilePath = "";
                var result = await AssignmentService.CreateAsync(assignment);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                if (assignmentFile != null && result.Data != null)
                {
                    var filePath = await _fileUploadService.UploadAssignmentFileAsync(
                        assignmentFile, result.Data.Id);

                    var updateDto = new UpdateAssignmentDTO
                    {
                        Id = result.Data.Id,
                        Title = result.Data.Title,
                        Description = result.Data.Description,
                        DueDate = result.Data.DueDate,
                        FilePath = filePath
                    };

                    result = await AssignmentService.UpdateAsync(updateDto);
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ServiceResponseDTO<ReadAssignmentDTO>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ServiceResponseDTO<ReadAssignmentDTO>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("submit-with-file")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> SubmitAssignmentWithFile(
            [FromForm] string assignmentId,
            [FromForm] IFormFile submissionFile)
        {
            try
            {
                var studentId = GetCurrentUserId() ?? "";

                if (submissionFile == null)
                    return BadRequest(new ServiceResponseDTO<StudentAssignmentDTO>
                    {
                        Success = false,
                        Message = "Submission file is required"
                    });

                var filePath = await _fileUploadService.UploadSubmissionFileAsync(
                    submissionFile, assignmentId, studentId);

                var submission = new SubmitAssignmentDTO
                {
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    FilePath = filePath
                };

                var result = await AssignmentService.SubmitAssignmentAsync(submission);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("download/{filePath}")]
        [Authorize]
        public async Task<ActionResult> DownloadFile(string filePath)
        {
            try
            {
                var decodedPath = Uri.UnescapeDataString(filePath);
                var file = await _fileUploadService.DownloadFileAsync(decodedPath);

                if (file == null)
                    return NotFound(new { message = "File not found" });

                return file;
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error downloading file: {ex.Message}" });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadAssignmentDTO>>> CreateAssignment(
            CreateAssignmentDTO assignment)
        {
            var instructorId = GetCurrentUserId() ?? "";
            assignment.InstructorId = instructorId;
            var result = await AssignmentService.CreateAsync(assignment);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<ReadAssignmentDTO>>> UpdateAssignment(
            UpdateAssignmentDTO assignment)
        {
            var result = await AssignmentService.UpdateAsync(assignment);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<List<ReadAssignmentDTO>>>> GetAssignmentsByCourse(
            string courseId)
        {
            var result = await AssignmentService.GetAssignmentsByCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("details/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<AssignmentDetailsDTO>>> GetAssignmentById(string id)
        {
            var result = await AssignmentService.GetAssignmentWithDetailsAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> SubmitAssignment(
            SubmitAssignmentDTO submission)
        {
            var studentId = GetCurrentUserId() ?? "";
            submission.StudentId = studentId;

            var result = await AssignmentService.SubmitAssignmentAsync(submission);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("grade")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> GradeAssignment(
            GradeAssignmentDTO grade)
        {
            if (string.IsNullOrEmpty(grade.StudentAssignmentId))
                return BadRequest(new ServiceResponseDTO<StudentAssignmentDTO>
                {
                    Success = false,
                    Message = "StudentAssignmentId is required"
                });

            var result = await AssignmentService.GradeAssignmentAsync(grade);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("student/{assignmentId}/{studentId}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> GetStudentAssignment(
            string assignmentId, string studentId)
        {
            var result = await AssignmentService.GetStudentAssignmentAsync(assignmentId, studentId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("submissions/{assignmentId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<List<StudentAssignmentDTO>>>> GetAssignmentSubmissions(
            string assignmentId)
        {
            var result = await AssignmentService.GetAssignmentSubmissionsAsync(assignmentId);
            return Ok(result);
        }

        [HttpGet("submission/{studentAssignmentId}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> GetStudentAssignmentById(
            string studentAssignmentId)
        {
            var result = await AssignmentService.GetStudentAssignmentByIdAsync(studentAssignmentId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("submissions/status/{assignmentId}/{status}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<List<StudentAssignmentDTO>>>> GetSubmissionsByStatus(
            string assignmentId, AssignmentStatus status)
        {
            var result = await AssignmentService.GetSubmissionsByStatusAsync(assignmentId, status);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<bool>>> DeleteAssignment(string id)
        {
            var assignmentResult = await AssignmentService.GetAssignmentWithDetailsAsync(id);

            if (assignmentResult.Success && assignmentResult.Data != null)
            {
                if (!string.IsNullOrEmpty(assignmentResult.Data.FilePath))
                {
                    await _fileUploadService.DeleteFileAsync(assignmentResult.Data.FilePath);
                }

                if (assignmentResult.Data.StudentSubmissions != null)
                {
                    foreach (var submission in assignmentResult.Data.StudentSubmissions)
                    {
                        if (!string.IsNullOrEmpty(submission.FilePath))
                        {
                            await _fileUploadService.DeleteFileAsync(submission.FilePath);
                        }
                    }
                }
            }

            var result = await AssignmentService.DeleteAssignmentAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("student/{studentId}/submissions")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<List<StudentAssignmentDTO>>>> GetStudentSubmissions(string studentId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != studentId)
            {
                return Forbid();
            }

            var result = await AssignmentService.GetStudentSubmissionsAsync(studentId);
            return Ok(result);
        }

        [HttpGet("student/{studentId}/all-assignments")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<List<StudentAllAssignmentsDTO>>>> GetStudentAllAssignments(string studentId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != studentId)
            {
                return Forbid();
            }

            var result = await AssignmentService.GetStudentAllAssignmentsAsync(studentId);
            return Ok(result);
        }

        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<List<ReadAssignmentDTO>>>> GetAssignmentsByInstructor(string instructorId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != instructorId)
            {
                return Forbid();
            }

            var result = await AssignmentService.GetAssignmentsByInstructorAsync(instructorId);
            return Ok(result);
        }
    }
}
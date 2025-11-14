using Domain.Enums; // ADD: For AssignmentStatus enum
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

        public AssignmentController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }

        private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private IAssignmentServices AssignmentService => _unitOfServices.Assignments;

        [HttpPost("create")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadAssignmentDTO>>> CreateAssignment(CreateAssignmentDTO assignment)
        {
            var instructorId = GetCurrentUserId() ?? "";
            assignment.InstructorId = instructorId;

            var result = await AssignmentService.CreateAsync(assignment);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadAssignmentDTO>>> UpdateAssignment(UpdateAssignmentDTO assignment)
        {
            var result = await AssignmentService.UpdateAsync(assignment);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<List<ReadAssignmentDTO>>>> GetAssignmentsByCourse(string courseId)
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
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> SubmitAssignment(SubmitAssignmentDTO submission)
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
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> GradeAssignment(GradeAssignmentDTO grade)
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
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> GetStudentAssignment(string assignmentId, string studentId)
        {
            var result = await AssignmentService.GetStudentAssignmentAsync(assignmentId, studentId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("submissions/{assignmentId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<List<StudentAssignmentDTO>>>> GetAssignmentSubmissions(string assignmentId)
        {
            var result = await AssignmentService.GetAssignmentSubmissionsAsync(assignmentId);
            return Ok(result);
        }
        [HttpGet("submission/{studentAssignmentId}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponseDTO<StudentAssignmentDTO>>> GetStudentAssignmentById(string studentAssignmentId)
        {
            var result = await AssignmentService.GetStudentAssignmentByIdAsync(studentAssignmentId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // NEW: Endpoint for getting submissions by status
        [HttpGet("submissions/status/{assignmentId}/{status}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<List<StudentAssignmentDTO>>>> GetSubmissionsByStatus(string assignmentId, AssignmentStatus status)
        {
            var result = await AssignmentService.GetSubmissionsByStatusAsync(assignmentId, status);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<bool>>> DeleteAssignment(string id)
        {
            try
            {
                var result = await AssignmentService.DeleteAsync(id);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ServiceResponseDTO<bool>
                {
                    Success = false,
                    Message = $"Error deleting assignment: {ex.Message}"
                });
            }
        }

    }
}
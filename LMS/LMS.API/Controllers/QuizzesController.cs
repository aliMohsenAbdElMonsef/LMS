using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Quiz;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;

        public QuizzesController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuizDTO>>> CreateQuiz(CreateQuizDTO quiz)
        {
            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            quiz.InstructorId = instructorId;
            var result = await _unitOfServices.Quizzes.CreateAsync(quiz);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuizDTO>>> UpdateQuiz(UpdateQuizDTO quiz)
        {
            var result = await _unitOfServices.Quizzes.UpdateAsync(quiz);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuizDTO>>> DeleteQuiz(string id)
        {
            var result = await _unitOfServices.Quizzes.DeleteAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuizDTO>>> GetQuizById(string id)
        {
            var result = await _unitOfServices.Quizzes.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<List<ReadQuizDTO>>>> GetAllQuizzes()
        {
            var result = await _unitOfServices.Quizzes.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<QuizResultDTO>>> SubmitQuiz(SubmitQuizDTO dto)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            dto.StudentId = studentId;
            var result = await _unitOfServices.Quizzes.SubmitQuizAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("start/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<bool>>> StartQuiz(string id)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _unitOfServices.Quizzes.StartQuizAsync(id, studentId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("take/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<QuizAttemptDTO>>> TakeQuiz(string id)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _unitOfServices.Quizzes.GetQuizForTakingAsync(id, studentId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>>> GetQuizzesByCourse(string courseId)
        {
            var result = await _unitOfServices.Quizzes.GetQuizzesByCourseAsync(courseId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>>> GetQuizzesByInstructor(string instructorId)
        {
            var result = await _unitOfServices.Quizzes.GetQuizzesByInstructorAsync(instructorId);
            return Ok(result);
        }

        [HttpGet("status/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<StudentQuizStatusDTO>>> GetQuizStatus(string id)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _unitOfServices.Quizzes.GetStudentQuizStatusAsync(id, studentId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }



        [HttpGet("results/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<QuizResultDTO>>> GetQuizResults(string id)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _unitOfServices.Quizzes.GetQuizResultAsync(id, studentId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("results/{id}/{studentId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<QuizResultDTO>>> GetStudentQuizResult(string id, string studentId)
        {
            var result = await _unitOfServices.Quizzes.GetQuizResultAsync(id, studentId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("submissions/{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>>> GetQuizSubmissions(string id)
        {
            var result = await _unitOfServices.Quizzes.GetQuizSubmissionsAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("my-quizzes")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>>> GetMyQuizzes()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _unitOfServices.Quizzes.GetStudentQuizzesAsync(studentId);
            return Ok(result);
        }

        [HttpPost("grade")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GradeQuiz([FromBody] ManualGradeDTO dto)
        {
            var result = await _unitOfServices.Quizzes.GradeQuizAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}

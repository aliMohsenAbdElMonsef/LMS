using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.CourseReview;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseReviewsController : ControllerBase
    {
        private readonly ICourseReviewService _reviewService;

        public CourseReviewsController(ICourseReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCourseReviewDTO>>> CreateReview(CreateCourseReviewDTO dto)
        {
            var result = await _reviewService.CreateReviewAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCourseReviewDTO>>> UpdateReview(UpdateCourseReviewDTO dto)
        {
            var result = await _reviewService.UpdateReviewAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCourseReviewDTO>>> DeleteReview(string id)
        {
            var result = await _reviewService.DeleteReviewAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<ReadCourseReviewDTO>>> GetReviewById(string id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCourseReviews(string courseId)
        {
            var result = await _reviewService.GetCourseReviewsAsync(courseId);
            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<ActionResult> GetStudentReviews(string studentId)
        {
            var result = await _reviewService.GetStudentReviewsAsync(studentId);
            return Ok(result);
        }

        [HttpGet("my-reviews")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> GetMyReviews()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _reviewService.GetStudentReviewsAsync(studentId);
            return Ok(result);
        }

        [HttpGet("stats/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<CourseRatingStatsDTO>>> GetCourseRatingStats(string courseId)
        {
            var result = await _reviewService.GetCourseRatingStatsAsync(courseId);
            return Ok(result);
        }

        [HttpPut("moderate/{reviewId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BasicResponseDTO>> ModerateReview(string reviewId, [FromQuery] bool approve)
        {
            var result = await _reviewService.ModerateReviewAsync(reviewId, approve);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}

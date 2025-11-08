using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;
        public CoursesController(
            IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }

        private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        private string? GetCurrentUserRole() => User.FindFirst(ClaimTypes.Role)?.Value;

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseDTO dto)
        {
            dto.AdminId = GetCurrentUserId() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var result = await _unitOfServices.Courses.CreateCourse(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = "Course created successfully",
                data = result.Data
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(string id)
        {
            var result = await _unitOfServices.Courses.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result.Data });
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCourses()
        {
            var result = await _unitOfServices.Courses.GetAllAsync();

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                data = result.Data,
                totalCount = result.Data?.Count() ?? 0
            });
        }

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse(string id, [FromForm] UpdateCourseDTO dto)
        {
            if (id != dto.Id)
                return BadRequest(new { success = false, message = "Course ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            

            var result = await _unitOfServices.Courses.UpdateAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = "Course updated successfully",
                data = result.Data
            });
        }

        [HttpPut("update-thumbnail/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourseThumbnail(string id, IFormFile thumbnailFile)
        {
            var result = await _unitOfServices.Courses.UpdateThumbnailAsync(id, thumbnailFile);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = "Thumbnail updated successfully",
                data = result.Data
            });
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            var result = await _unitOfServices.Courses.DeleteAsync(id);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = "Course deleted successfully" });
        }
        [HttpGet("isenrolled")]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<IActionResult> IsEnrolledIn(RequestEnrollIntoCourseDTO dto)
        {
            var Role = GetCurrentUserRole();
            if (Role == null)
            {
                return BadRequest(new ServiceResponseDTO<bool>
                {
                    Success = false,
                    Message = "Couldn't find user role."
                });
            }
            if(Role == "Instructor")
            {
                var result = await _unitOfServices.InstructorEnrollIntoCourse.IsEnrolledIn(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            else
            {
                var result = await _unitOfServices.StudentEnrollIntoCourse.IsEnrolledIn(dto);
                return result.Success ? Ok(result) : BadRequest(result);

            }
        }
        
    }
}

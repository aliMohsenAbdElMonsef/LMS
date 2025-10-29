using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
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

        public CoursesController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }
        

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid data",
                        errors = ModelState
                    });
                }
                if (string.IsNullOrEmpty(dto.AdminId))
                {
                    dto.AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                var result = await _unitOfServices.Courses.CreateCourseWithScheduleAsync(dto);

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "Course created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var result = await _unitOfServices.Courses.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet("{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourse(string courseId)
        {
            try
            {
                var result = await _unitOfServices.Courses.GetByIdAsync(courseId);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("{courseId}/lectures")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseLectures(string courseId)
        {
            try
            {
                var result = await _unitOfServices.Courses.GetCourseLecturesAsync(courseId);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet("{courseId}/schedule")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseSchedule(string courseId)
        {
            try
            {
                var result = await _unitOfServices.Courses.GetCourseScheduleAsync(courseId);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpPut("{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse(string courseId, [FromBody] UpdateCourseDTO dto)
        {
            try
            {
                if (courseId != dto.Id)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Course ID mismatch"
                    });

                if (!ModelState.IsValid)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid data"
                    });

                var result = await _unitOfServices.Courses.UpdateAsync(dto);
                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "Course updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpDelete("{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(string courseId)
        {
            try
            {
                await _unitOfServices.Courses.DeleteAsync(courseId);
                return Ok(new
                {
                    success = true,
                    message = "Course deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}

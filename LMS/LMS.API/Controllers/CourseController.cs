using LMS.BusinessLogic.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseServices _courseServices;

        public CourseController(ICourseServices courseServices)
        {
            _courseServices = courseServices;
        }

        [HttpGet("all")]
        [Authorize] // Require authentication
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseServices.GetAllAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCourseById(string id)
        {
            var course = await _courseServices.GetByIdAsync(id);
            if (course == null)
                return NotFound();
            return Ok(course);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] LMS.BusinessLogic.DTOs.Course.CreateCourseDTO dto)
        {
            try
            {
                var result = await _courseServices.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse(string id, [FromBody] LMS.BusinessLogic.DTOs.Course.UpdateCourseDTO dto)
        {
            try
            {
                dto.Id = id;
                var result = await _courseServices.UpdateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            try
            {
                await _courseServices.DeleteAsync(id);
                return Ok(new { Success = true, Message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}

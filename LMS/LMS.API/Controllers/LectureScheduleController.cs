using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.LectureSchedule;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LectureScheduleController : ControllerBase
    {
        private readonly ILectureScheduleService _scheduleService;

        public LectureScheduleController(ILectureScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateLectureScheduleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _scheduleService.CreateScheduleAsync(dto, GetUserId(), GetUserRole());
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSchedule(string id, [FromBody] UpdateLectureScheduleDTO dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _scheduleService.UpdateScheduleAsync(dto, GetUserId(), GetUserRole());
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchedule(string id)
        {
            var result = await _scheduleService.GetScheduleByIdAsync(id, GetUserId(), GetUserRole());
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseSchedules(string courseId)
        {
            var result = await _scheduleService.GetCourseSchedulesAsync(courseId, GetUserId(), GetUserRole());
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSchedule(string id)
        {
            var result = await _scheduleService.DeleteScheduleAsync(id, GetUserId(), GetUserRole());
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateLectures(string id)
        {
            var result = await _scheduleService.GenerateLecturesFromScheduleAsync(id, GetUserId(), GetUserRole());
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }
    }
}

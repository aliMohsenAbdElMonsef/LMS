using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Recieve.Lectures;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LectureController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;

        public LectureController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }

        private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        private string? GetCurrentUserRole() => User.FindFirst(ClaimTypes.Role)?.Value;

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLectures()
        {
            var result = await _unitOfServices.Lectures.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
        

        [HttpGet("get_lecture/{lectureId}")]
        public async Task<IActionResult> GetLecture(GetLectureRecieveDTO dto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.GetByIdAsync(dto.courseId, dto.LectureId, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseLectures(string courseId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.GetCourseLecturesAsync(courseId, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("instructor/{instructorId}")]
        public async Task<IActionResult> GetInstructorLectures(string instructorId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
             //need update
            var result = await _unitOfServices.Lectures.GetInstructorLecturesAsync(instructorId, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("upcoming/course/{courseId}")]
        public async Task<IActionResult> GetUpcomingCourseLectures(string courseId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.GetUpcomingLecturesAsync(courseId, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("today/course/{courseId}")]
        public async Task<IActionResult> GetTodayLectures(string courseId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.GetTodayLecturesAsync(courseId, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("check-conflict")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CheckLectureConflict(
            [FromQuery] string courseId,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime,
            [FromQuery] string excludeLectureId = null)
        {
            var result = await _unitOfServices.Lectures.CheckLectureConflictAsync(courseId, date, startTime, endTime, excludeLectureId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse<GetLectureDTO>("Invalid data", GetModelStateErrors()));

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.CreateAsync(dto, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{lectureId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLecture(string lectureId, [FromBody] UpdateLectureDTO dto)
        {
            if (lectureId != dto.Id)
                return BadRequest(CreateErrorResponse<GetLectureDTO>("Lecture ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(CreateErrorResponse<GetLectureDTO>("Invalid data", GetModelStateErrors()));

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.UpdateAsync(dto, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{lectureId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLecture(string lectureId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                await _unitOfServices.Lectures.DeleteAsync(lectureId, userId, userRole);

                var result = CreateSuccessResponse<GetLectureDTO>(null, "Lecture deleted successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(CreateErrorResponse<GetLectureDTO>($"Error deleting lecture: {ex.Message}"));
            }
        }

        private ServiceResponseDTO<T> CreateErrorResponse<T>(string message, List<string> errors = null)
        {
            return new ServiceResponseDTO<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        private ServiceResponseDTO<T> CreateSuccessResponse<T>(T data, string message = "")
        {
            return new ServiceResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }
    }
}
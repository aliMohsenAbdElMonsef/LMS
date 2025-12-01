using Application.DTOs.User;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }
        // ---------------Admin---------------
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userServices.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingUsers()
        {
            var users = await _userServices.GetPendingUsers();
            return Ok(users);
        }

        [HttpGet("current")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCurrentUsers()
        {
            var users = await _userServices.GetCurrentUsers();
            return Ok(users);
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var users = await _userServices.ApproveUserAsync(id);
            return Ok(users);
        }
        [HttpPost("deny/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DenyUser(string id)
        {
            var users = await _userServices.DenyUserAsync(id);
            return Ok(users);
        }

        [HttpGet("counts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserCounts()
        {
            var users = await _userServices.GetAllUsers();
            var studentCount = users.Count(u => u.ApplyAs == Domain.Enums.UserType.Student && u.Status == Domain.Enums.ApplicationStatus.Approved);
            var instructorCount = users.Count(u => u.ApplyAs == Domain.Enums.UserType.Instructor && u.Status == Domain.Enums.ApplicationStatus.Approved);
            return Ok(new { StudentCount = studentCount, InstructorCount = instructorCount });
        }

        // ------------------------------
        // ---------------All---------------
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromForm] SignUpDTO dto)
        {
            var response = await _userServices.CreateUserAsync(dto);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutUser()
        {
            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var response = await _userServices.LogoutUser(token, userId);
            return Ok(response);
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser(LoginDTO dto)
        {
            var response = await _userServices.LoginUser(dto);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [HttpGet("{userId}/role")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserRole(string userId)
        {
            var response = await _userServices.FindByIdAsync(userId);
            if (response.Success)
            {
                var newres = new ServiceResponseDTO<string>
                {
                    Success = true,
                    Message = "Role gotten successfully.",
                    Data = response.Data.ApplyAs.ToString()
                };
                return Ok(newres);
            }
            return BadRequest(response);
            
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            var response = await _userServices.ForgotPasswordAsync(dto);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var response = await _userServices.ResetPasswordAsync(dto);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("profile/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var response = await _userServices.FindByIdAsync(userId);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response);
        }

        [HttpPut("profile/update/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string userId, [FromForm] UpdateUserDTO dto)
        {
            if (userId != dto.Id)
            {
                return BadRequest("User ID mismatch");
            }

            var response = await _userServices.UpdateProfileAsync(userId, dto);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var response = await _userServices.ChangePasswordAsync(dto.UserId, dto.CurrentPassword, dto.NewPassword);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("stats/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserStats(string userId)
        {
            var response = await _userServices.GetUserStatsAsync(userId);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response);
        }
    }
}
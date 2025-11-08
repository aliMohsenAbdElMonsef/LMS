using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
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
    }
}
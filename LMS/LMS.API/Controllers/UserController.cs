using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IBlackListedTokensServices _blackListedTokensService;
        private readonly ITokenServices _tokenServices;
        public UserController(IUserServices userServices, IBlackListedTokensServices blacklistedServices, ITokenServices tokenServices)
        {
            _userServices = userServices;
            _blackListedTokensService = blacklistedServices;
            _tokenServices = tokenServices;
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
            var token = HttpContext.Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var expiryDate = _tokenServices.GetExpiryFromToken(token);

            await _blackListedTokensService.AddTokenAsync(token, expiryDate, userId);

            return Ok(new { message = "Logged out successfully" });
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
    }
}
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

        // --------------- Test Endpoint ---------------
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            Console.WriteLine("[UserController] 🔥 Test endpoint hit successfully!");
            return Ok(new { message = "API is working", timestamp = DateTime.Now });
        }

        // --------------- Admin ---------------
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                Console.WriteLine($"[UserController] GetAllUsers called at: {DateTime.Now}");

                // Debug authentication info
                Console.WriteLine($"[UserController] User Identity: {User.Identity?.Name}");
                Console.WriteLine($"[UserController] IsAuthenticated: {User.Identity?.IsAuthenticated}");

                // Check roles
                var roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();

                Console.WriteLine($"[UserController] Roles found: {string.Join(", ", roles)}");

                if (!roles.Contains("Admin"))
                {
                    Console.WriteLine($"[UserController] ❌ User does not have Admin role");
                    return Forbid();
                }

                Console.WriteLine($"[UserController] Calling user service...");
                var users = await _userServices.GetAllUsers();
                Console.WriteLine($"[UserController] Retrieved {users.Count()} users");

                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserController] Error: {ex.Message}");
                Console.WriteLine($"[UserController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
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
        // --------------- All ---------------
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
    }
}
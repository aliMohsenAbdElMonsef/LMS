using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenServices _tokenServices;
        public TokenController(ITokenServices tokenServices)
        {
            _tokenServices = tokenServices;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
        {
            var result = await _tokenServices.RefreshAccessTokenAsync(request.RefreshToken, request.UserId);
            if (result.Success)
            {
                return Ok(result);
            }
            return Unauthorized(result);
        }
    }
}

using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Notification;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<ServiceResponseDTO<ReadNotificationDTO>>> CreateNotification(CreateNotificationDTO dto)
        {
            var result = await _notificationService.CreateNotificationAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("my-notifications")]
        [Authorize]
        public async Task<ActionResult> GetMyNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(result);
        }

        [HttpGet("unread")]
        [Authorize]
        public async Task<ActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        [Authorize]
        public async Task<ActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        [HttpPut("mark-read/{id}")]
        [Authorize]
        public async Task<ActionResult<BasicResponseDTO>> MarkAsRead(string id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("mark-all-read")]
        [Authorize]
        public async Task<ActionResult<BasicResponseDTO>> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteNotification(string id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}

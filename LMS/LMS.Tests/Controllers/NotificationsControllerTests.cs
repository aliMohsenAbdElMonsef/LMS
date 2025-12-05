using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Notification;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class NotificationsControllerTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly NotificationsController _controller;

        public NotificationsControllerTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new NotificationsController(_mockNotificationService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateNotification_ReturnsOk_WhenSuccessful()
        {

            var dto = new CreateNotificationDTO { Title = "Test" };
            var responseDto = new ServiceResponseDTO<ReadNotificationDTO> { Success = true };

            _mockNotificationService.Setup(s => s.CreateNotificationAsync(dto))
                .ReturnsAsync(responseDto);


            var result = await _controller.CreateNotification(dto);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMyNotifications_ReturnsOk()
        {

            var responseDto = new ServiceResponseDTO<IEnumerable<ReadNotificationDTO>> { Success = true };

            _mockNotificationService.Setup(s => s.GetUserNotificationsAsync("user-id"))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetMyNotifications();


            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsOk_WhenSuccessful()
        {

            var id = "1";
            var responseDto = new BasicResponseDTO { Success = true };

            _mockNotificationService.Setup(s => s.MarkAsReadAsync(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.MarkAsRead(id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

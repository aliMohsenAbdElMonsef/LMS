using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.DaySchedule;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class DaySchedulControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<ICourseDayScheduleServices> _mockDayScheduleServices;
        private readonly DaySchedulController.DaySchedulesController _controller;

        public DaySchedulControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockDayScheduleServices = new Mock<ICourseDayScheduleServices>();

            _mockUnitOfServices.Setup(u => u.DaySchedules).Returns(_mockDayScheduleServices.Object);

            _controller = new DaySchedulController.DaySchedulesController(_mockUnitOfServices.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-id"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetCourseSchedules_ReturnsOk_WhenSuccessful()
        {

            var courseId = "course1";
            var schedules = new List<GetDayScheduleDTO>
            {
                new GetDayScheduleDTO { Id = "1", DayOfWeek = 1, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) }
            };

            _mockDayScheduleServices.Setup(s => s.GetCourseSchedulesAsync(courseId))
                .ReturnsAsync(schedules);


            var result = await _controller.GetCourseSchedules(courseId);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCourseSchedules_ReturnsNotFound_WhenExceptionThrown()
        {

            var courseId = "course1";

            _mockDayScheduleServices.Setup(s => s.GetCourseSchedulesAsync(courseId))
                .ThrowsAsync(new Exception("Course not found"));


            var result = await _controller.GetCourseSchedules(courseId);


            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task GetDaySchedule_ReturnsOk_WhenFound()
        {

            var scheduleId = "schedule1";
            var responseDto = new ServiceResponseDTO<GetDayScheduleDTO>
            {
                Success = true,
                Data = new GetDayScheduleDTO { Id = scheduleId, DayOfWeek = 1 }
            };

            _mockDayScheduleServices.Setup(s => s.GetByIdAsync(scheduleId))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetDaySchedule(scheduleId);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetDaySchedule_ReturnsNotFound_WhenExceptionThrown()
        {

            var scheduleId = "schedule1";

            _mockDayScheduleServices.Setup(s => s.GetByIdAsync(scheduleId))
                .ThrowsAsync(new Exception("Schedule not found"));


            var result = await _controller.GetDaySchedule(scheduleId);


            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateDaySchedule_ReturnsOk_WhenSuccessful()
        {

            var createDto = new CreateDayScheduleDTO
            {
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11)
            };

            var responseDto = new ServiceResponseDTO<GetDayScheduleDTO>
            {
                Success = true,
                Data = new GetDayScheduleDTO { Id = "1", DayOfWeek = 1 }
            };

            _mockDayScheduleServices.Setup(s => s.CreateAsync(It.IsAny<CreateDayScheduleDTO>()))
                .ReturnsAsync(responseDto);


            var result = await _controller.CreateDaySchedule(createDto);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task CreateDaySchedule_ReturnsBadRequest_WhenModelInvalid()
        {

            var createDto = new CreateDayScheduleDTO();
            _controller.ModelState.AddModelError("DayOfWeek", "Required");


            var result = await _controller.CreateDaySchedule(createDto);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDaySchedule_ReturnsOk_WhenSuccessful()
        {

            var scheduleId = "schedule1";
            var updateDto = new UpdateDayScheduleDTO
            {
                Id = scheduleId,
                DayOfWeek = 2,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                InstructorId = "instructor1"
            };

            var responseDto = new ServiceResponseDTO<GetDayScheduleDTO>
            {
                Success = true,
                Data = new GetDayScheduleDTO { Id = scheduleId }
            };

            _mockDayScheduleServices.Setup(s => s.UpdateAsync(It.IsAny<UpdateDayScheduleDTO>()))
                .ReturnsAsync(responseDto);


            var result = await _controller.UpdateDaySchedule(scheduleId, updateDto);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateDaySchedule_ReturnsBadRequest_WhenIdMismatch()
        {

            var scheduleId = "schedule1";
            var updateDto = new UpdateDayScheduleDTO
            {
                Id = "different-id",
                DayOfWeek = 2,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                InstructorId = "instructor1"
            };


            var result = await _controller.UpdateDaySchedule(scheduleId, updateDto);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteDaySchedule_ReturnsOk_WhenSuccessful()
        {

            var scheduleId = "schedule1";
            var responseDto = new ServiceResponseDTO<GetDayScheduleDTO>
            {
                Success = true
            };

            _mockDayScheduleServices.Setup(s => s.DeleteAsync(scheduleId))
                .ReturnsAsync(responseDto);


            var result = await _controller.DeleteDaySchedule(scheduleId);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteDaySchedule_ReturnsBadRequest_WhenExceptionThrown()
        {

            var scheduleId = "schedule1";

            _mockDayScheduleServices.Setup(s => s.DeleteAsync(scheduleId))
                .ThrowsAsync(new Exception("Delete failed"));


            var result = await _controller.DeleteDaySchedule(scheduleId);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}


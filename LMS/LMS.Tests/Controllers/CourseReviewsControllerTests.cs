using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.CourseReview;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class CourseReviewsControllerTests
    {
        private readonly Mock<ICourseReviewService> _mockReviewService;
        private readonly CourseReviewsController _controller;

        public CourseReviewsControllerTests()
        {
            _mockReviewService = new Mock<ICourseReviewService>();
            _controller = new CourseReviewsController(_mockReviewService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "student-id"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateReview_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var dto = new CreateCourseReviewDTO { CourseId = "course1", Rating = 5 };
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.CreateReviewAsync(dto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateReview(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateReview_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var dto = new UpdateCourseReviewDTO { Id = "1", Rating = 4 };
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.UpdateReviewAsync(dto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateReview(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteReview_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.DeleteReviewAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteReview(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetReviewById_ReturnsOk_WhenFound()
        {
            // Arrange
            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.GetReviewByIdAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetReviewById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCourseReviews_ReturnsOk()
        {
            // Arrange
            var courseId = "course1";
            var responseDto = new ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>> { Success = true };

            _mockReviewService.Setup(s => s.GetCourseReviewsAsync(courseId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetCourseReviews(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCourseRatingStats_ReturnsOk()
        {
            // Arrange
            var courseId = "course1";
            var responseDto = new ServiceResponseDTO<CourseRatingStatsDTO> { Success = true };

            _mockReviewService.Setup(s => s.GetCourseRatingStatsAsync(courseId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetCourseRatingStats(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task ModerateReview_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var reviewId = "1";
            var responseDto = new BasicResponseDTO { Success = true };

            _mockReviewService.Setup(s => s.ModerateReviewAsync(reviewId, true))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.ModerateReview(reviewId, true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

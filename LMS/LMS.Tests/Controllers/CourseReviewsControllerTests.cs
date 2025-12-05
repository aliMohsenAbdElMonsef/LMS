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

            var dto = new CreateCourseReviewDTO { CourseId = "course1", Rating = 5 };
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.CreateReviewAsync(dto))
                .ReturnsAsync(responseDto);


            var result = await _controller.CreateReview(dto);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateReview_ReturnsOk_WhenSuccessful()
        {

            var dto = new UpdateCourseReviewDTO { Id = "1", Rating = 4 };
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.UpdateReviewAsync(dto))
                .ReturnsAsync(responseDto);


            var result = await _controller.UpdateReview(dto);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteReview_ReturnsOk_WhenSuccessful()
        {

            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.DeleteReviewAsync(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.DeleteReview(id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetReviewById_ReturnsOk_WhenFound()
        {

            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadCourseReviewDTO> { Success = true };

            _mockReviewService.Setup(s => s.GetReviewByIdAsync(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetReviewById(id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCourseReviews_ReturnsOk()
        {

            var courseId = "course1";
            var responseDto = new ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>> { Success = true };

            _mockReviewService.Setup(s => s.GetCourseReviewsAsync(courseId))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetCourseReviews(courseId);


            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCourseRatingStats_ReturnsOk()
        {

            var courseId = "course1";
            var responseDto = new ServiceResponseDTO<CourseRatingStatsDTO> { Success = true };

            _mockReviewService.Setup(s => s.GetCourseRatingStatsAsync(courseId))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetCourseRatingStats(courseId);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task ModerateReview_ReturnsOk_WhenSuccessful()
        {

            var reviewId = "1";
            var responseDto = new BasicResponseDTO { Success = true };

            _mockReviewService.Setup(s => s.ModerateReviewAsync(reviewId, true))
                .ReturnsAsync(responseDto);


            var result = await _controller.ModerateReview(reviewId, true);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

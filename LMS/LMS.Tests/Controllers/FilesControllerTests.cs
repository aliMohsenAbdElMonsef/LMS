using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class FilesControllerTests
    {
        private readonly Mock<IFileService> _mockFileService;
        private readonly FilesController _controller;

        public FilesControllerTests()
        {
            _mockFileService = new Mock<IFileService>();
            _controller = new FilesController(_mockFileService.Object);

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
        public async Task GetCourseThumbnail_ReturnsFileResult_WhenFound()
        {

            var fileName = "test.jpg";
            var fileResult = new FileContentResult(new byte[] { 1, 2, 3 }, "image/jpeg");

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ReturnsAsync(fileResult);


            var result = await _controller.GetCourseThumbnail(fileName);


            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task GetCourseThumbnail_ReturnsDefaultThumbnail_WhenNotFound()
        {

            var fileName = "nonexistent.jpg";
            var defaultFileResult = new FileContentResult(new byte[] { 1, 2, 3 }, "image/jpeg");

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ReturnsAsync((FileResult?)null);
            _mockFileService.Setup(s => s.GetDefaultThumbnailAsync())
                .ReturnsAsync(defaultFileResult);


            var result = await _controller.GetCourseThumbnail(fileName);


            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task GetCourseThumbnail_ReturnsNotFound_WhenNoThumbnailAvailable()
        {

            var fileName = "nonexistent.jpg";

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ReturnsAsync((FileResult?)null);
            _mockFileService.Setup(s => s.GetDefaultThumbnailAsync())
                .ReturnsAsync((FileResult?)null);


            var result = await _controller.GetCourseThumbnail(fileName);


            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task GetCourseThumbnail_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {

            var fileName = "invalid.jpg";

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ThrowsAsync(new ArgumentException("Invalid file name"));


            var result = await _controller.GetCourseThumbnail(fileName);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsOk_WhenSuccessful()
        {

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.jpg");

            var responseDto = new FileUploadResponseDTO
            {
                Success = true,
                FileName = "test.jpg",
                Message = "Uploaded successfully"
            };

            _mockFileService.Setup(s => s.SaveCourseThumbnailAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(responseDto);


            var result = await _controller.UploadCourseThumbnail(fileMock.Object);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsBadRequest_WhenFileIsNull()
        {

            var result = await _controller.UploadCourseThumbnail(null);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsBadRequest_WhenFileIsEmpty()
        {

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);


            var result = await _controller.UploadCourseThumbnail(fileMock.Object);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsBadRequest_WhenServiceReturnsFailure()
        {

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);

            var responseDto = new FileUploadResponseDTO
            {
                Success = false,
                Message = "Upload failed"
            };

            _mockFileService.Setup(s => s.SaveCourseThumbnailAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(responseDto);


            var result = await _controller.UploadCourseThumbnail(fileMock.Object);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCourseThumbnail_ReturnsOk_WhenSuccessful()
        {

            var fileName = "test.jpg";
            var responseDto = new FileOperationResponseDTO
            {
                Success = true,
                Message = "Deleted successfully"
            };

            _mockFileService.Setup(s => s.DeleteCourseThumbnailAsync(fileName))
                .ReturnsAsync(responseDto);


            var result = await _controller.DeleteCourseThumbnail(fileName);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var response = okResult.Value.ToString();
            Assert.Contains("success", response);
        }

        [Fact]
        public async Task DeleteCourseThumbnail_ReturnsBadRequest_WhenServiceReturnsFailure()
        {

            var fileName = "test.jpg";
            var responseDto = new FileOperationResponseDTO
            {
                Success = false,
                Message = "Delete failed"
            };

            _mockFileService.Setup(s => s.DeleteCourseThumbnailAsync(fileName))
                .ReturnsAsync(responseDto);


            var result = await _controller.DeleteCourseThumbnail(fileName);


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCourseThumbnail_ReturnsInternalServerError_WhenExceptionThrown()
        {

            var fileName = "test.jpg";

            _mockFileService.Setup(s => s.DeleteCourseThumbnailAsync(fileName))
                .ThrowsAsync(new Exception("Unexpected error"));


            var result = await _controller.DeleteCourseThumbnail(fileName);


            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}


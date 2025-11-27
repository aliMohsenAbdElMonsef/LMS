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
            // Arrange
            var fileName = "test.jpg";
            var fileResult = new FileContentResult(new byte[] { 1, 2, 3 }, "image/jpeg");

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ReturnsAsync(fileResult);

            // Act
            var result = await _controller.GetCourseThumbnail(fileName);

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task GetCourseThumbnail_ReturnsDefaultThumbnail_WhenNotFound()
        {
            // Arrange
            var fileName = "nonexistent.jpg";
            var defaultFileResult = new FileContentResult(new byte[] { 1, 2, 3 }, "image/jpeg");

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ReturnsAsync((FileResult?)null);
            _mockFileService.Setup(s => s.GetDefaultThumbnailAsync())
                .ReturnsAsync(defaultFileResult);

            // Act
            var result = await _controller.GetCourseThumbnail(fileName);

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task GetCourseThumbnail_ReturnsNotFound_WhenNoThumbnailAvailable()
        {
            // Arrange
            var fileName = "nonexistent.jpg";

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ReturnsAsync((FileResult?)null);
            _mockFileService.Setup(s => s.GetDefaultThumbnailAsync())
                .ReturnsAsync((FileResult?)null);

            // Act
            var result = await _controller.GetCourseThumbnail(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task GetCourseThumbnail_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var fileName = "invalid.jpg";

            _mockFileService.Setup(s => s.GetCourseThumbnailAsync(fileName))
                .ThrowsAsync(new ArgumentException("Invalid file name"));

            // Act
            var result = await _controller.GetCourseThumbnail(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsOk_WhenSuccessful()
        {
            // Arrange
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

            // Act
            var result = await _controller.UploadCourseThumbnail(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsBadRequest_WhenFileIsNull()
        {
            // Act
            var result = await _controller.UploadCourseThumbnail(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsBadRequest_WhenFileIsEmpty()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.UploadCourseThumbnail(fileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadCourseThumbnail_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);

            var responseDto = new FileUploadResponseDTO
            {
                Success = false,
                Message = "Upload failed"
            };

            _mockFileService.Setup(s => s.SaveCourseThumbnailAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UploadCourseThumbnail(fileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCourseThumbnail_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var fileName = "test.jpg";
            var responseDto = new FileOperationResponseDTO
            {
                Success = true,
                Message = "Deleted successfully"
            };

            _mockFileService.Setup(s => s.DeleteCourseThumbnailAsync(fileName))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteCourseThumbnail(fileName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            // Controller returns anonymous object: new { success = true, message = "..." }
            var response = okResult.Value.ToString();
            Assert.Contains("success", response);
        }

        [Fact]
        public async Task DeleteCourseThumbnail_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var fileName = "test.jpg";
            var responseDto = new FileOperationResponseDTO
            {
                Success = false,
                Message = "Delete failed"
            };

            _mockFileService.Setup(s => s.DeleteCourseThumbnailAsync(fileName))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteCourseThumbnail(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCourseThumbnail_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var fileName = "test.jpg";

            _mockFileService.Setup(s => s.DeleteCourseThumbnailAsync(fileName))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.DeleteCourseThumbnail(fileName);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}


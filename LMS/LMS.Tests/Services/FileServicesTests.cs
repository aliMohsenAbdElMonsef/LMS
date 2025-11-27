using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class FileServicesTests
    {
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private readonly FileService _fileService;
        private readonly string _testWebRootPath;

        public FileServicesTests()
        {
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _testWebRootPath = Path.Combine(Path.GetTempPath(), "LMSTests");
            _webHostEnvironmentMock.Setup(e => e.WebRootPath).Returns(_testWebRootPath);

            _fileService = new FileService(_webHostEnvironmentMock.Object);

            // Create test directories
            Directory.CreateDirectory(Path.Combine(_testWebRootPath, "uploads", "course", "thumbnails"));
        }

        [Fact]
        public async Task SaveCourseThumbnailAsync_ShouldReturnFailure_WhenFileIsNull()
        {
            // Act
            var result = await _fileService.SaveCourseThumbnailAsync(null);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No file provided", result.Message);
        }

        [Fact]
        public async Task SaveCourseThumbnailAsync_ShouldReturnFailure_WhenFileIsEmpty()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);
            fileMock.Setup(f => f.FileName).Returns("test.jpg");

            // Act
            var result = await _fileService.SaveCourseThumbnailAsync(fileMock.Object);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No file provided", result.Message);
        }

        [Fact]
        public async Task SaveCourseThumbnailAsync_ShouldReturnFailure_WhenFileTypeIsInvalid()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.exe"); // Invalid extension

            // Act
            var result = await _fileService.SaveCourseThumbnailAsync(fileMock.Object);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid file type. Only image files are allowed.", result.Message);
        }

        [Fact]
        public async Task SaveCourseThumbnailAsync_ShouldReturnFailure_WhenFileSizeExceeds5MB()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB
            fileMock.Setup(f => f.FileName).Returns("test.jpg");

            // Act
            var result = await _fileService.SaveCourseThumbnailAsync(fileMock.Object);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("File size cannot exceed 5MB.", result.Message);
        }

        [Fact]
        public async Task SaveCourseThumbnailAsync_ShouldReturnSuccess_WhenFileIsValid()
        {
            // Arrange
            var content = "Fake image content";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream));

            // Act
            var result = await _fileService.SaveCourseThumbnailAsync(fileMock.Object);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("File uploaded successfully", result.Message);
            Assert.NotNull(result.FileName);
            Assert.Contains(".jpg", result.FileName);

            // Cleanup
            if (!string.IsNullOrEmpty(result.FileName))
            {
                var filePath = Path.Combine(_testWebRootPath, "uploads", "course", "thumbnails", result.FileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact]
        public async Task DeleteCourseThumbnailAsync_ShouldReturnFailure_WhenFileNameIsInvalid()
        {
            // Act - test with path traversal attempt
            var result = await _fileService.DeleteCourseThumbnailAsync("../../../etc/passwd");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid file name", result.Message);
        }

        [Fact]
        public async Task DeleteCourseThumbnailAsync_ShouldReturnFailure_WhenFileNameIsEmpty()
        {
            // Act
            var result = await _fileService.DeleteCourseThumbnailAsync("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid file name", result.Message);
        }

        [Fact]
        public async Task DeleteCourseThumbnailAsync_ShouldReturnFailure_WhenFileDoesNotExist()
        {
            // Act
            var result = await _fileService.DeleteCourseThumbnailAsync("nonexistent.jpg");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("File not found", result.Message);
        }

        [Fact]
        public async Task DeleteCourseThumbnailAsync_ShouldReturnSuccess_WhenFileExists()
        {
            // Arrange - create a test file
            var fileName = "test-delete.jpg";
            var filePath = Path.Combine(_testWebRootPath, "uploads", "course", "thumbnails", fileName);
            await File.WriteAllTextAsync(filePath, "test content");

            // Act
            var result = await _fileService.DeleteCourseThumbnailAsync(fileName);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("File deleted successfully", result.Message);
            Assert.False(File.Exists(filePath));
        }
    }
}

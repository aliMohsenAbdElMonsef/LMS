using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class AssignmentControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<IAssignmentServices> _mockAssignmentServices;
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly Mock<IUserServices> _mockUserServices;
        private readonly AssignmentController _controller;

        public AssignmentControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockAssignmentServices = new Mock<IAssignmentServices>();
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockUserServices = new Mock<IUserServices>();

            _mockUnitOfServices.Setup(u => u.Assignments).Returns(_mockAssignmentServices.Object);
            _mockUnitOfServices.Setup(u => u.Users).Returns(_mockUserServices.Object);

            _controller = new AssignmentController(_mockUnitOfServices.Object, _mockFileUploadService.Object);
            
            // Mock User Claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateAssignment_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateAssignmentDTO { Title = "Test Assignment" };
            var responseDto = new ServiceResponseDTO<ReadAssignmentDTO> 
            { 
                Success = true, 
                Data = new ReadAssignmentDTO { Title = "Test Assignment" } 
            };

            _mockAssignmentServices.Setup(s => s.CreateAsync(It.IsAny<CreateAssignmentDTO>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateAssignment(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadAssignmentDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task CreateAssignmentWithFile_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateAssignmentDTO { Title = "Test Assignment" };
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");

            var createResponse = new ServiceResponseDTO<ReadAssignmentDTO> 
            { 
                Success = true, 
                Data = new ReadAssignmentDTO { Id = "1", Title = "Test Assignment" } 
            };

            _mockAssignmentServices.Setup(s => s.CreateAsync(It.IsAny<CreateAssignmentDTO>()))
                .ReturnsAsync(createResponse);
            
            _mockFileUploadService.Setup(s => s.UploadAssignmentFileAsync(It.IsAny<IFormFile>(), "1"))
                .ReturnsAsync("path/to/file");

            var updateResponse = new ServiceResponseDTO<ReadAssignmentDTO>
            {
                Success = true,
                Data = new ReadAssignmentDTO { Id = "1", Title = "Test Assignment", FilePath = "path/to/file" }
            };

            _mockAssignmentServices.Setup(s => s.UpdateAsync(It.IsAny<UpdateAssignmentDTO>()))
                .ReturnsAsync(updateResponse);

            // Act
            var result = await _controller.CreateAssignmentWithFile(createDto, fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadAssignmentDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task UpdateAssignment_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var updateDto = new UpdateAssignmentDTO { Id = "1", Title = "Updated" };
            var responseDto = new ServiceResponseDTO<ReadAssignmentDTO> { Success = true };

            _mockAssignmentServices.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateAssignment(updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(((ServiceResponseDTO<ReadAssignmentDTO>)okResult.Value).Success);
        }

        [Fact]
        public async Task GetAssignmentsByCourse_ReturnsOk()
        {
            // Arrange
            var courseId = "course1";
            var responseDto = new ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>> { Success = true, Data = new List<ReadAssignmentDTO>() };

            _mockAssignmentServices.Setup(s => s.GetAssignmentsByCourseAsync(courseId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetAssignmentsByCourse(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAssignmentById_ReturnsOk_WhenFound()
        {
            // Arrange
            var id = "1";
            var responseDto = new ServiceResponseDTO<AssignmentDetailsDTO> { Success = true };

            _mockAssignmentServices.Setup(s => s.GetAssignmentWithDetailsAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetAssignmentById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task SubmitAssignment_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var submission = new SubmitAssignmentDTO { AssignmentId = "1" };
            var responseDto = new ServiceResponseDTO<StudentAssignmentDTO> { Success = true };

            _mockAssignmentServices.Setup(s => s.SubmitAssignmentAsync(It.IsAny<SubmitAssignmentDTO>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.SubmitAssignment(submission);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GradeAssignment_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var grade = new GradeAssignmentDTO { StudentAssignmentId = "1", Grade = 100 };
            var responseDto = new ServiceResponseDTO<StudentAssignmentDTO> { Success = true };

            _mockAssignmentServices.Setup(s => s.GradeAssignmentAsync(grade))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GradeAssignment(grade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteAssignment_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var id = "1";
            var detailsResponse = new ServiceResponseDTO<AssignmentDetailsDTO> 
            { 
                Success = true, 
                Data = new AssignmentDetailsDTO { FilePath = "path/to/file" } 
            };
            var deleteResponse = new ServiceResponseDTO<bool> { Success = true };

            _mockAssignmentServices.Setup(s => s.GetAssignmentWithDetailsAsync(id))
                .ReturnsAsync(detailsResponse);
            _mockFileUploadService.Setup(s => s.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockAssignmentServices.Setup(s => s.DeleteAssignmentAsync(id))
                .ReturnsAsync(deleteResponse);

            // Act
            var result = await _controller.DeleteAssignment(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

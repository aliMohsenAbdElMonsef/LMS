using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Quiz;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LMS.Tests.Controllers
{
    public class QuizzesControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<IQuizServices> _mockQuizServices;
        private readonly QuizzesController _controller;

        public QuizzesControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockQuizServices = new Mock<IQuizServices>();

            // Setup the Quizzes property of UnitOfServices to return the mock QuizServices
            _mockUnitOfServices.Setup(u => u.Quizzes).Returns(_mockQuizServices.Object);

            _controller = new QuizzesController(_mockUnitOfServices.Object);
        }

        private void SetUser(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task CreateQuiz_ReturnsOk_WhenCreationIsSuccessful()
        {
            // Arrange
            SetUser("user1");
            var createDto = new CreateQuizDTO { Title = "Test Quiz" };
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = true, 
                Data = new ReadQuizDTO { Title = "Test Quiz" } 
            };

            _mockQuizServices.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateQuiz(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Test Quiz", returnValue.Data.Title);
        }

        [Fact]
        public async Task CreateQuiz_ReturnsBadRequest_WhenCreationFails()
        {
            // Arrange
            SetUser("user1");
            var createDto = new CreateQuizDTO { Title = "Test Quiz" };
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = false, 
                Message = "Creation failed" 
            };

            _mockQuizServices.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateQuiz(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Creation failed", returnValue.Message);
        }

        [Fact]
        public async Task UpdateQuiz_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateDto = new UpdateQuizDTO { Id = "1", Title = "Updated Quiz" };
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = true, 
                Data = new ReadQuizDTO { Id = "1", Title = "Updated Quiz" } 
            };

            _mockQuizServices.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateQuiz(updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Updated Quiz", returnValue.Data.Title);
        }

        [Fact]
        public async Task UpdateQuiz_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var updateDto = new UpdateQuizDTO { Id = "1", Title = "Updated Quiz" };
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = false, 
                Message = "Update failed" 
            };

            _mockQuizServices.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateQuiz(updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Update failed", returnValue.Message);
        }

        [Fact]
        public async Task DeleteQuiz_ReturnsOk_WhenDeleteIsSuccessful()
        {
            // Arrange
            var quizId = "1";
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = true, 
                Data = new ReadQuizDTO { Id = quizId } 
            };

            _mockQuizServices.Setup(s => s.DeleteAsync(quizId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteQuiz(quizId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task DeleteQuiz_ReturnsNotFound_WhenDeleteFails()
        {
            // Arrange
            var quizId = "1";
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = false, 
                Message = "Quiz not found" 
            };

            _mockQuizServices.Setup(s => s.DeleteAsync(quizId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteQuiz(quizId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(notFoundResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Quiz not found", returnValue.Message);
        }

        [Fact]
        public async Task GetQuizById_ReturnsOk_WhenQuizExists()
        {
            // Arrange
            var quizId = "1";
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = true, 
                Data = new ReadQuizDTO { Id = quizId, Title = "Test Quiz" } 
            };

            _mockQuizServices.Setup(s => s.GetByIdAsync(quizId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetQuizById(quizId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(quizId, returnValue.Data.Id);
        }

        [Fact]
        public async Task GetQuizById_ReturnsNotFound_WhenQuizDoesNotExist()
        {
            // Arrange
            var quizId = "1";
            var responseDto = new ServiceResponseDTO<ReadQuizDTO> 
            { 
                Success = false, 
                Message = "Quiz not found" 
            };

            _mockQuizServices.Setup(s => s.GetByIdAsync(quizId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetQuizById(quizId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuizDTO>>(notFoundResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Quiz not found", returnValue.Message);
        }

        [Fact]
        public async Task GetAllQuizzes_ReturnsOk_WithListOfQuizzes()
        {
            // Arrange
            var quizzes = new List<ReadQuizDTO>
            {
                new ReadQuizDTO { Id = "1", Title = "Quiz 1" },
                new ReadQuizDTO { Id = "2", Title = "Quiz 2" }
            };
            var responseDto = new ServiceResponseDTO<IEnumerable<ReadQuizDTO>> 
            { 
                Success = true, 
                Data = quizzes 
            };

            _mockQuizServices.Setup(s => s.GetAllAsync())
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetAllQuizzes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(2, returnValue.Data.Count());
        }

        [Fact]
        public async Task SubmitQuiz_ReturnsOk_WhenSubmissionIsSuccessful()
        {
            // Arrange
            SetUser("user1");
            var submitDto = new SubmitQuizDTO { QuizId = "1" };
            var responseDto = new ServiceResponseDTO<QuizResultDTO> 
            { 
                Success = true, 
                Data = new QuizResultDTO { EarnedPoints = 100 } 
            };

            _mockQuizServices.Setup(s => s.SubmitQuizAsync(submitDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.SubmitQuiz(submitDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<QuizResultDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(100, returnValue.Data.EarnedPoints);
        }

        [Fact]
        public async Task SubmitQuiz_ReturnsBadRequest_WhenSubmissionFails()
        {
            // Arrange
            SetUser("user1");
            var submitDto = new SubmitQuizDTO { QuizId = "1" };
            var responseDto = new ServiceResponseDTO<QuizResultDTO> 
            { 
                Success = false, 
                Message = "Submission failed" 
            };

            _mockQuizServices.Setup(s => s.SubmitQuizAsync(submitDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.SubmitQuiz(submitDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<QuizResultDTO>>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Submission failed", returnValue.Message);
        }
    }
}

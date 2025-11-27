using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Question;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class QuestionsControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<IQuestionServices> _mockQuestionServices;
        private readonly QuestionsController _controller;

        public QuestionsControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockQuestionServices = new Mock<IQuestionServices>();

            _mockUnitOfServices.Setup(u => u.Questions).Returns(_mockQuestionServices.Object);

            _controller = new QuestionsController(_mockUnitOfServices.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "instructor-id"),
                new Claim(ClaimTypes.Role, "Instructor")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateQuestion_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var createDto = new CreateQuestionDTO
            {
                QuizId = "quiz1",
                Text = "Test Question",
                OptionA = "Option A",
                OptionB = "Option B",
                OptionC = "Option C",
                OptionD = "Option D",
                CorrectAnswer = Domain.Enums.Options.OptionA,
                Points = 10
            };

            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Data = new ReadQuestionDTO { Id = "1", Text = "Test Question" }
            };

            _mockQuestionServices.Setup(s => s.CreateAsync(It.IsAny<CreateQuestionDTO>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateQuestion(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuestionDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task CreateQuestion_ReturnsBadRequest_WhenUnsuccessful()
        {
            // Arrange
            var createDto = new CreateQuestionDTO
            {
                QuizId = "quiz1",
                Text = "Test Question"
            };

            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = false,
                Message = "Validation failed"
            };

            _mockQuestionServices.Setup(s => s.CreateAsync(It.IsAny<CreateQuestionDTO>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateQuestion(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateQuestion_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var updateDto = new UpdateQuestionDTO
            {
                Id = "1",
                QuizId = "quiz1",
                Text = "Updated Question",
                OptionA = "Option A",
                OptionB = "Option B",
                OptionC = "Option C",
                OptionD = "Option D",
                CorrectAnswer = Domain.Enums.Options.OptionB,
                Points = 15
            };

            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Data = new ReadQuestionDTO { Id = "1", Text = "Updated Question" }
            };

            _mockQuestionServices.Setup(s => s.UpdateAsync(It.IsAny<UpdateQuestionDTO>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateQuestion(updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<ReadQuestionDTO>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task UpdateQuestion_ReturnsBadRequest_WhenUnsuccessful()
        {
            // Arrange
            var updateDto = new UpdateQuestionDTO { Id = "1" };

            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = false,
                Message = "Question not found"
            };

            _mockQuestionServices.Setup(s => s.UpdateAsync(It.IsAny<UpdateQuestionDTO>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateQuestion(updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteQuestion_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true
            };

            _mockQuestionServices.Setup(s => s.DeleteAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteQuestion(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteQuestion_ReturnsNotFound_WhenUnsuccessful()
        {
            // Arrange
            var id = "nonexistent";
            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = false,
                Message = "Question not found"
            };

            _mockQuestionServices.Setup(s => s.DeleteAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.DeleteQuestion(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetQuestionById_ReturnsOk_WhenFound()
        {
            // Arrange
            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Data = new ReadQuestionDTO { Id = id, Text = "Test Question" }
            };

            _mockQuestionServices.Setup(s => s.GetByIdAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetQuestionById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetQuestionById_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var id = "nonexistent";
            var responseDto = new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = false,
                Message = "Question not found"
            };

            _mockQuestionServices.Setup(s => s.GetByIdAsync(id))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetQuestionById(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllQuestions_ReturnsOk()
        {
            // Arrange
            var questions = new List<ReadQuestionDTO>
            {
                new ReadQuestionDTO { Id = "1", Text = "Question 1" },
                new ReadQuestionDTO { Id = "2", Text = "Question 2" }
            };
            var responseDto = new ServiceResponseDTO<IEnumerable<ReadQuestionDTO>>
            {
                Success = true,
                Data = questions
            };

            _mockQuestionServices.Setup(s => s.GetAllAsync())
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetAllQuestions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponseDTO<IEnumerable<ReadQuestionDTO>>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(2, returnValue.Data?.Count() ?? 0);
        }
    }
}


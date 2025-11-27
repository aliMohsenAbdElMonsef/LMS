using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Question;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class QuestionServicesTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IQuestionRepository> _mockQuestionRepo;
        private readonly QuestionServices _service;

        public QuestionServicesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockQuestionRepo = new Mock<IQuestionRepository>();

            _mockUnitOfWork.Setup(u => u.Questions).Returns(_mockQuestionRepo.Object);

            _service = new QuestionServices(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task CreateAsync_ReturnsSuccess_WhenQuestionCreated()
        {
            // Arrange
            var createDto = new CreateQuestionDTO
            {
                QuizId = "quiz1",
                Text = "What is 2+2?",
                OptionA = "3",
                OptionB = "4",
                OptionC = "5",
                OptionD = "6",
                CorrectAnswer = Options.OptionB,
                Points = 10
            };

            _mockQuestionRepo.Setup(r => r.CreateAsync(It.IsAny<Question>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("What is 2+2?", result.Data.Text);
            Assert.Equal("OptionB", result.Data.CorrectAnswer);
            Assert.Equal(10, result.Data.Points);
            _mockQuestionRepo.Verify(r => r.CreateAsync(It.IsAny<Question>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            var createDto = new CreateQuestionDTO
            {
                QuizId = "quiz1",
                Text = "Test Question",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = Options.OptionA,
                Points = 5
            };

            _mockQuestionRepo.Setup(r => r.CreateAsync(It.IsAny<Question>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(createDto));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsSuccess_WhenQuestionUpdated()
        {
            // Arrange
            var questionId = "question1";
            var existingQuestion = new Question
            {
                Id = questionId,
                QuizId = "quiz1",
                Text = "Original Question",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = Options.OptionA,
                Points = 10
            };

            var updateDto = new UpdateQuestionDTO
            {
                Id = questionId,
                QuizId = "quiz1",
                Text = "Updated Question",
                OptionA = "New A",
                CorrectAnswer = Options.OptionB,
                Points = 15
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync(existingQuestion);
            _mockQuestionRepo.Setup(r => r.UpdateAsync(It.IsAny<Question>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(updateDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Question", result.Data.Text);
            Assert.Equal("New A", result.Data.OptionA);
            Assert.Equal("OptionB", result.Data.CorrectAnswer);
            Assert.Equal(15, result.Data.Points);
            _mockQuestionRepo.Verify(r => r.FindByIdAsync(questionId), Times.Once);
            _mockQuestionRepo.Verify(r => r.UpdateAsync(It.IsAny<Question>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenQuestionNotFound()
        {
            // Arrange
            var updateDto = new UpdateQuestionDTO
            {
                Id = "nonexistent",
                QuizId = "quiz1"
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync("nonexistent"))
                .ReturnsAsync((Question?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.UpdateAsync(updateDto));
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_PartialUpdate_WhenSomeFieldsNull()
        {
            // Arrange
            var questionId = "question1";
            var existingQuestion = new Question
            {
                Id = questionId,
                QuizId = "quiz1",
                Text = "Original Question",
                OptionA = "Original A",
                OptionB = "Original B",
                OptionC = "Original C",
                OptionD = "Original D",
                CorrectAnswer = Options.OptionA,
                Points = 10
            };

            var updateDto = new UpdateQuestionDTO
            {
                Id = questionId,
                QuizId = "quiz1",
                Text = "Updated Text",
                OptionA = null, // Not updating
                CorrectAnswer = null, // Not updating
                Points = null // Not updating
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync(existingQuestion);
            _mockQuestionRepo.Setup(r => r.UpdateAsync(It.IsAny<Question>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(updateDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated Text", result.Data.Text);
            Assert.Equal("Original A", result.Data.OptionA); // Should remain unchanged
            Assert.Equal("OptionA", result.Data.CorrectAnswer); // Should remain unchanged
            Assert.Equal(10, result.Data.Points); // Should remain unchanged
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenQuestionDeleted()
        {
            // Arrange
            var questionId = "question1";
            var existingQuestion = new Question
            {
                Id = questionId,
                QuizId = "quiz1",
                Text = "Test Question",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = Options.OptionA,
                Points = 10
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync(existingQuestion);
            _mockQuestionRepo.Setup(r => r.DeleteWithIDAsync(questionId))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(questionId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Entity Deleted Succesfully.", result.Message);
            _mockQuestionRepo.Verify(r => r.FindByIdAsync(questionId), Times.Once);
            _mockQuestionRepo.Verify(r => r.DeleteWithIDAsync(questionId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFailure_WhenQuestionNotFound()
        {
            // Arrange
            var questionId = "nonexistent";

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync((Question?)null);

            // Act
            var result = await _service.DeleteAsync(questionId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
            _mockQuestionRepo.Verify(r => r.DeleteWithIDAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSuccess_WhenQuestionFound()
        {
            // Arrange
            var questionId = "question1";
            var question = new Question
            {
                Id = questionId,
                QuizId = "quiz1",
                Text = "Test Question",
                OptionA = "Option A",
                OptionB = "Option B",
                OptionC = "Option C",
                OptionD = "Option D",
                CorrectAnswer = Options.OptionC,
                Points = 20
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync(question);

            // Act
            var result = await _service.GetByIdAsync(questionId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(questionId, result.Data.Id);
            Assert.Equal("Test Question", result.Data.Text);
            Assert.Equal("OptionC", result.Data.CorrectAnswer);
            Assert.Equal(20, result.Data.Points);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsFailure_WhenQuestionNotFound()
        {
            // Arrange
            var questionId = "nonexistent";

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync((Question?)null);

            // Act
            var result = await _service.GetByIdAsync(questionId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllQuestions()
        {
            // Arrange
            var questions = new List<Question>
            {
                new Question
                {
                    Id = "1",
                    QuizId = "quiz1",
                    Text = "Question 1",
                    OptionA = "A1",
                    OptionB = "B1",
                    OptionC = "C1",
                    OptionD = "D1",
                    CorrectAnswer = Options.OptionA,
                    Points = 10
                },
                new Question
                {
                    Id = "2",
                    QuizId = "quiz1",
                    Text = "Question 2",
                    OptionA = "A2",
                    OptionB = "B2",
                    OptionC = "C2",
                    OptionD = "D2",
                    CorrectAnswer = Options.OptionB,
                    Points = 15
                }
            };

            _mockQuestionRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(questions);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Contains(result.Data, q => q.Id == "1" && q.Text == "Question 1");
            Assert.Contains(result.Data, q => q.Id == "2" && q.Text == "Question 2");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoQuestions()
        {
            // Arrange
            var emptyList = new List<Question>();

            _mockQuestionRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task MapToReadDTO_ConvertsCorrectAnswerToString()
        {
            // Arrange
            var question = new Question
            {
                Id = "1",
                QuizId = "quiz1",
                Text = "Test",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = Options.OptionD,
                Points = 10
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync("1"))
                .ReturnsAsync(question);

            // Act
            var result = await _service.GetByIdAsync("1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("OptionD", result.Data.CorrectAnswer);
        }
    }
}


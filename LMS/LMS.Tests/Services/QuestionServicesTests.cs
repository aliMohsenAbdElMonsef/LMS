using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
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
        private readonly Mock<IMapper> _mockMapper;
        private readonly IQuestionServices _service;

        public QuestionServicesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockQuestionRepo = new Mock<IQuestionRepository>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.Questions).Returns(_mockQuestionRepo.Object);

            _service = new QuestionServices(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateAsync_ReturnsSuccess_WhenQuestionCreated()
        {

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

            _mockMapper.Setup(m => m.Map<Question>(createDto)).Returns(new Question());
            _mockMapper.Setup(m => m.Map<ReadQuestionDTO>(It.IsAny<Question>()))
                .Returns(new ReadQuestionDTO
                {
                    Text = createDto.Text,
                    CorrectAnswer = createDto.CorrectAnswer.ToString(),
                    Points = createDto.Points
                });


            var result = await _service.CreateAsync(createDto);


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


            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(createDto));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsSuccess_WhenQuestionUpdated()
        {

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

            _mockMapper.Setup(m => m.Map<ReadQuestionDTO>(existingQuestion))
                .Returns(new ReadQuestionDTO
                {
                    Text = updateDto.Text,
                    OptionA = updateDto.OptionA,
                    CorrectAnswer = "OptionB",
                    Points = updateDto.Points.Value
                });


            var result = await _service.UpdateAsync(updateDto);


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

            var updateDto = new UpdateQuestionDTO
            {
                Id = "nonexistent",
                QuizId = "quiz1"
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync("nonexistent"))
                .ReturnsAsync((Question?)null);


            var result = await _service.UpdateAsync(updateDto);


            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_PartialUpdate_WhenSomeFieldsNull()
        {

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
                OptionA = null,
                CorrectAnswer = null,
                Points = null
            };

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync(existingQuestion);
            _mockQuestionRepo.Setup(r => r.UpdateAsync(It.IsAny<Question>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<ReadQuestionDTO>(existingQuestion))
                .Returns(new ReadQuestionDTO
                {
                    Text = updateDto.Text,
                    OptionA = existingQuestion.OptionA,
                    CorrectAnswer = existingQuestion.CorrectAnswer.ToString(),
                    Points = existingQuestion.Points
                });


            var result = await _service.UpdateAsync(updateDto);


            Assert.True(result.Success);
            Assert.Equal("Updated Text", result.Data.Text);
            Assert.Equal("Original A", result.Data.OptionA);
            Assert.Equal("OptionA", result.Data.CorrectAnswer);
            Assert.Equal(10, result.Data.Points);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenQuestionDeleted()
        {

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
            _mockQuestionRepo.Setup(r => r.DeleteByEntityAsync(existingQuestion))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);


            var result = await _service.DeleteAsync(questionId);


            Assert.True(result.Success);
            Assert.Equal("Entity Deleted Succesfully.", result.Message);
            _mockQuestionRepo.Verify(r => r.FindByIdAsync(questionId), Times.Once);
            _mockQuestionRepo.Verify(r => r.DeleteByEntityAsync(existingQuestion), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFailure_WhenQuestionNotFound()
        {

            var questionId = "nonexistent";

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync((Question?)null);


            var result = await _service.DeleteAsync(questionId);


            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
            _mockQuestionRepo.Verify(r => r.DeleteByEntityAsync(It.IsAny<Question>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSuccess_WhenQuestionFound()
        {

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

            _mockMapper.Setup(m => m.Map<ReadQuestionDTO>(question))
                .Returns(new ReadQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    CorrectAnswer = question.CorrectAnswer.ToString(),
                    Points = question.Points
                });


            var result = await _service.GetByIdAsync(questionId);


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

            var questionId = "nonexistent";

            _mockQuestionRepo.Setup(r => r.FindByIdAsync(questionId))
                .ReturnsAsync((Question?)null);


            var result = await _service.GetByIdAsync(questionId);


            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllQuestions()
        {

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

            _mockMapper.Setup(m => m.Map<IEnumerable<ReadQuestionDTO>>(questions))
                .Returns(new List<ReadQuestionDTO>
                {
                    new ReadQuestionDTO { Id = "1", Text = "Question 1" },
                    new ReadQuestionDTO { Id = "2", Text = "Question 2" }
                });


            var result = await _service.GetAllAsync();


            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Contains(result.Data, q => q.Id == "1" && q.Text == "Question 1");
            Assert.Contains(result.Data, q => q.Id == "2" && q.Text == "Question 2");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoQuestions()
        {

            var emptyList = new List<Question>();

            _mockQuestionRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(emptyList);

            _mockMapper.Setup(m => m.Map<IEnumerable<ReadQuestionDTO>>(emptyList))
                .Returns(new List<ReadQuestionDTO>());


            var result = await _service.GetAllAsync();


            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task MapToReadDTO_ConvertsCorrectAnswerToString()
        {

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

            _mockMapper.Setup(m => m.Map<ReadQuestionDTO>(question))
                .Returns(new ReadQuestionDTO { CorrectAnswer = "OptionD" });


            var result = await _service.GetByIdAsync("1");


            Assert.True(result.Success);
            Assert.Equal("OptionD", result.Data.CorrectAnswer);
        }
    }
}


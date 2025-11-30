using LMS.MVC.Controllers;
using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class MVCQuizControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<IQuizService> _mockQuizService;
        private readonly QuizController _controller;

        public MVCQuizControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockQuizService = new Mock<IQuizService>();

            _mockUnitOfServices.Setup(u => u.QuizService).Returns(_mockQuizService.Object);

            _controller = new QuizController(_mockUnitOfServices.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithSortedQuizzes()
        {
            // Arrange
            var courseId = "course1";
            var quiz1 = new QuizItemViewModel { Id = "1", Title = "Quiz 1", EndDate = DateTime.Now.AddDays(5) };
            var quiz2 = new QuizItemViewModel { Id = "2", Title = "Quiz 2", EndDate = DateTime.Now.AddDays(1) }; // Sooner
            var quiz3 = new QuizItemViewModel { Id = "3", Title = "Quiz 3", EndDate = DateTime.Now.AddDays(10) };

            var quizzes = new List<QuizItemViewModel> { quiz1, quiz2, quiz3 };
            var serviceResult = new SuccessServiceResult<IEnumerable<QuizItemViewModel>>
            {
                Success = true,
                Data = quizzes
            };

            _mockQuizService.Setup(s => s.GetQuizzesByCourseAsync(courseId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Index(courseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QuizListViewModel>(viewResult.Model);
            
            var sortedQuizzes = model.Quizzes.ToList();
            Assert.Equal(3, sortedQuizzes.Count);
            Assert.Equal("Quiz 2", sortedQuizzes[0].Title); // Earliest deadline first
            Assert.Equal("Quiz 1", sortedQuizzes[1].Title);
            Assert.Equal("Quiz 3", sortedQuizzes[2].Title);
        }
    }
}

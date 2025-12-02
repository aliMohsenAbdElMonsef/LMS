using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LMS.MVC.Controllers;
using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class MVCQuizControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<IQuizService> _mockQuizService;
        private readonly Mock<IEnrollmentService> _mockEnrollmentService;
        private readonly QuizController _controller;

        public MVCQuizControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockQuizService = new Mock<IQuizService>();
            _mockEnrollmentService = new Mock<IEnrollmentService>();

            _mockUnitOfServices.Setup(u => u.QuizService).Returns(_mockQuizService.Object);
            _mockUnitOfServices.Setup(u => u.EnrollmentService).Returns(_mockEnrollmentService.Object);

            _controller = new QuizController(_mockUnitOfServices.Object);
            
            // Setup TempData
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        private void SetupUser(string role, string userId = "user1")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
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

        [Fact]
        public async Task Details_RedirectsToCourse_WhenStudentNotEnrolled()
        {
            // Arrange
            var quizId = "quiz1";
            var courseId = "course1";
            var userId = "student1";
            
            SetupUser("Student", userId);

            var quizViewModel = new QuizItemViewModel 
            { 
                Id = quizId, 
                CourseId = courseId,
                Title = "Test Quiz" 
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .ReturnsAsync(new SuccessServiceResult<QuizItemViewModel> { Success = true, Data = quizViewModel });

            _mockEnrollmentService.Setup(s => s.IsApprovedEnrollmentAsync(userId, courseId))
                .ReturnsAsync(false); // Not enrolled

            // Act
            var result = await _controller.Details(quizId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Course", redirectResult.ControllerName);
            Assert.Equal(courseId, redirectResult.RouteValues["id"]);
            Assert.Equal("You must be enrolled in the course to view this quiz.", _controller.TempData["Error"]);
        }

        [Fact]
        public async Task Details_ReturnsView_WhenStudentIsEnrolled()
        {
            // Arrange
            var quizId = "quiz1";
            var courseId = "course1";
            var userId = "student1";
            
            SetupUser("Student", userId);

            var quizViewModel = new QuizItemViewModel 
            { 
                Id = quizId, 
                CourseId = courseId,
                Title = "Test Quiz" 
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .ReturnsAsync(new SuccessServiceResult<QuizItemViewModel> { Success = true, Data = quizViewModel });

            _mockEnrollmentService.Setup(s => s.IsApprovedEnrollmentAsync(userId, courseId))
                .ReturnsAsync(true); // Enrolled

            _mockQuizService.Setup(s => s.GetQuizStatusAsync(quizId))
                .ReturnsAsync(new SuccessServiceResult<LMS.BusinessLogic.DTOs.Quiz.StudentQuizStatusDTO> 
                { 
                    Success = true, 
                    Data = new LMS.BusinessLogic.DTOs.Quiz.StudentQuizStatusDTO { Status = Domain.Enums.QuizStatus.NotStarted } 
                });

            // Act
            var result = await _controller.Details(quizId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QuizItemViewModel>(viewResult.Model);
            Assert.Equal(quizId, model.Id);
        }

        [Fact]
        public async Task Details_RedirectsToCourse_WhenInstructorNotEnrolled()
        {
            // Arrange
            var quizId = "quiz1";
            var courseId = "course1";
            var userId = "instructor1";
            
            SetupUser("Instructor", userId);

            var quizViewModel = new QuizItemViewModel 
            { 
                Id = quizId, 
                CourseId = courseId,
                Title = "Test Quiz" 
            };

            _mockQuizService.Setup(s => s.GetQuizByIdAsync(quizId))
                .ReturnsAsync(new SuccessServiceResult<QuizItemViewModel> { Success = true, Data = quizViewModel });

            _mockEnrollmentService.Setup(s => s.IsApprovedEnrollmentAsync(userId, courseId))
                .ReturnsAsync(false); // Not enrolled

            // Act
            var result = await _controller.Details(quizId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Course", redirectResult.ControllerName);
            Assert.Equal(courseId, redirectResult.RouteValues["id"]);
            Assert.Equal("You must be enrolled in the course to view this quiz.", _controller.TempData["Error"]);
        }
    }
}

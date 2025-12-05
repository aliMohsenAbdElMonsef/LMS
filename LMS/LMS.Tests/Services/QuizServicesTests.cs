using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Notification;
using LMS.BusinessLogic.DTOs.Quiz;
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
    public class QuizServicesTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IQuizRepository> _mockQuizRepo;
        private readonly Mock<IStudentEnrollIntoCourseRepository> _mockEnrollmentRepo;
        private readonly QuizServices _service;

        public QuizServicesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockQuizRepo = new Mock<IQuizRepository>();
            _mockEnrollmentRepo = new Mock<IStudentEnrollIntoCourseRepository>();

            _mockUnitOfWork.Setup(u => u.Quizzes).Returns(_mockQuizRepo.Object);
            _mockUnitOfWork.Setup(u => u.StudentEnrollments).Returns(_mockEnrollmentRepo.Object);

            _service = new QuizServices(_mockUnitOfWork.Object, _mockNotificationService.Object, _mockEmailService.Object);
        }

        #region StartQuizAsync Tests

        [Fact]
        public async Task StartQuizAsync_ReturnsError_WhenStudentNotEnrolled()
        {

            var quizId = "quiz1";
            var studentId = "student1";
            var courseId = "course1";

            var quiz = CreateTestQuiz();
            quiz.Id = quizId;
            quiz.CourseId = courseId;

            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            _mockEnrollmentRepo.Setup(r => r.IsStudentEnrolledInCourseAsync(studentId, courseId))
                .ReturnsAsync(false);


            var result = await _service.StartQuizAsync(quizId, studentId);


            Assert.False(result.Success);
            Assert.Equal("You must be enrolled in the course to take this quiz.", result.Message);
        }

        [Fact]
        public async Task StartQuizAsync_Success_WhenStudentEnrolled()
        {

            var quizId = "quiz1";
            var studentId = "student1";
            var courseId = "course1";

            var quiz = CreateTestQuiz();
            quiz.Id = quizId;
            quiz.CourseId = courseId;

            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            _mockEnrollmentRepo.Setup(r => r.IsStudentEnrolledInCourseAsync(studentId, courseId))
                .ReturnsAsync(true);

            var emptyStudentQuizzes = new List<StudentQuiz>().BuildMockDbSet().Object;
            _mockUnitOfWork.Setup(u => u.GetQueryable<StudentQuiz>()).Returns(emptyStudentQuizzes);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);


            var result = await _service.StartQuizAsync(quizId, studentId);


            Assert.True(result.Success);
        }

        #endregion

        #region GetQuizForTakingAsync Tests

        [Fact]
        public async Task GetQuizForTakingAsync_ReturnsError_WhenStudentNotEnrolled()
        {

            var quizId = "quiz1";
            var studentId = "student1";
            var courseId = "course1";

            var quiz = CreateTestQuiz();
            quiz.Id = quizId;
            quiz.CourseId = courseId;

            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            _mockEnrollmentRepo.Setup(r => r.IsStudentEnrolledInCourseAsync(studentId, courseId))
                .ReturnsAsync(false);


            var result = await _service.GetQuizForTakingAsync(quizId, studentId);


            Assert.False(result.Success);
            Assert.Equal("You must be enrolled in the course to take this quiz.", result.Message);
        }

        #endregion

        #region SubmitQuizAsync Tests

        [Fact]
        public async Task SubmitQuizAsync_ReturnsError_WhenQuizNotFound()
        {

            var dto = new SubmitQuizDTO { QuizId = "non-existent", StudentId = "student1" };
            
            var emptyQuizzes = new List<Quiz>().BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(emptyQuizzes);


            var result = await _service.SubmitQuizAsync(dto);


            Assert.False(result.Success);
            Assert.Equal("Quiz not found.", result.Message);
        }

        [Fact]
        public async Task SubmitQuizAsync_ReturnsError_WhenQuizNotStarted()
        {

            var quiz = CreateTestQuiz(
                startDate: DateTime.UtcNow.AddDays(1),
                endDate: DateTime.UtcNow.AddDays(7)
            );

            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO { QuizId = quiz.Id, StudentId = "student1" };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.False(result.Success);
            Assert.Equal("Quiz is not currently available.", result.Message);
        }

        [Fact]
        public async Task SubmitQuizAsync_ReturnsError_WhenQuizExpired()
        {

            var quiz = CreateTestQuiz(
                startDate: DateTime.UtcNow.AddDays(-7),
                endDate: DateTime.UtcNow.AddDays(-1)
            );

            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO { QuizId = quiz.Id, StudentId = "student1" };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.False(result.Success);
            Assert.Equal("Quiz is not currently available.", result.Message);
        }

        [Fact]
        public async Task SubmitQuizAsync_CalculatesGrade_AllCorrectAnswers()
        {

            var quiz = CreateTestQuiz();
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = quiz.Questions.Select(q => new StudentAnswerDTO
                {
                    QuestionId = q.Id,
                    SelectedAnswer = q.CorrectAnswer
                }).ToList()
            };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(100.0, result.Data.Percentage);
            Assert.Equal(100, result.Data.Grade);
            Assert.Equal(quiz.Questions.Count, result.Data.CorrectAnswers);
            Assert.Equal(quiz.Questions.Count, result.Data.TotalQuestions);
        }

        [Fact]
        public async Task SubmitQuizAsync_CalculatesGrade_AllWrongAnswers()
        {

            var quiz = CreateTestQuiz();
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = quiz.Questions.Select(q => new StudentAnswerDTO
                {
                    QuestionId = q.Id,
                    SelectedAnswer = GetWrongAnswer(q.CorrectAnswer)
                }).ToList()
            };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(0.0, result.Data.Percentage);
            Assert.Equal(0, result.Data.Grade);
            Assert.Equal(0, result.Data.CorrectAnswers);
            Assert.Equal(0, result.Data.EarnedPoints);
        }

        [Fact]
        public async Task SubmitQuizAsync_CalculatesGrade_HalfCorrect()
        {

            var quiz = CreateTestQuiz(questionCount: 4);
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var answers = new List<StudentAnswerDTO>();
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var question = quiz.Questions.ElementAt(i);
                answers.Add(new StudentAnswerDTO
                {
                    QuestionId = question.Id,
                    SelectedAnswer = i < 2 ? question.CorrectAnswer : GetWrongAnswer(question.CorrectAnswer)
                });
            }

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = answers
            };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(50.0, result.Data.Percentage);
            Assert.Equal(50, result.Data.Grade);
            Assert.Equal(2, result.Data.CorrectAnswers);
            Assert.Equal(20, result.Data.EarnedPoints);
            Assert.Equal(40, result.Data.TotalPoints);
        }

        [Fact]
        public async Task SubmitQuizAsync_HandlesNoAnswers()
        {

            var quiz = CreateTestQuiz();
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = new List<StudentAnswerDTO>()
            };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(0.0, result.Data.Percentage);
            Assert.Equal(0, result.Data.Grade);
            Assert.Equal(0, result.Data.CorrectAnswers);
            Assert.All(result.Data.QuestionResults, qr => Assert.Equal("Not Answered", qr.SelectedAnswer));
        }

        [Fact]
        public async Task SubmitQuizAsync_CalculatesGrade_DifferentPointValues()
        {

            var quiz = CreateTestQuizWithDifferentPoints();
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);


            var answers = new List<StudentAnswerDTO>();
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var question = quiz.Questions.ElementAt(i);
                answers.Add(new StudentAnswerDTO
                {
                    QuestionId = question.Id,
                    SelectedAnswer = i < 2 ? question.CorrectAnswer : GetWrongAnswer(question.CorrectAnswer)
                });
            }

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = answers
            };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(2, result.Data.CorrectAnswers);
            Assert.Equal(15, result.Data.EarnedPoints);
            Assert.Equal(30, result.Data.TotalPoints);
            Assert.Equal(50.0, result.Data.Percentage);
            Assert.Equal(50, result.Data.Grade);
        }

        [Fact]
        public async Task SubmitQuizAsync_RoundsGradeCorrectly()
        {

            var quiz = CreateTestQuiz(questionCount: 3);
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);


            var answers = new List<StudentAnswerDTO>();
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var question = quiz.Questions.ElementAt(i);
                answers.Add(new StudentAnswerDTO
                {
                    QuestionId = question.Id,
                    SelectedAnswer = i < 2 ? question.CorrectAnswer : GetWrongAnswer(question.CorrectAnswer)
                });
            }

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = answers
            };

            SetupMockForSubmission();


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(67, result.Data.Grade);
        }

        [Fact]
        public async Task SubmitQuizAsync_SendsNotification()
        {

            var quiz = CreateTestQuiz();
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = quiz.Questions.Select(q => new StudentAnswerDTO
                {
                    QuestionId = q.Id,
                    SelectedAnswer = q.CorrectAnswer
                }).ToList()
            };

            SetupMockForSubmission();

            _mockNotificationService.Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDTO>()))
                .ReturnsAsync(new ServiceResponseDTO<ReadNotificationDTO> { Success = true });


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            _mockNotificationService.Verify(n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDTO>(dto =>
                    dto.UserId == "student1" &&
                    dto.Title == "Quiz Completed" &&
                    dto.Type == NotificationType.QuizResult
                )), Times.Once);
        }

        [Fact]
        public async Task SubmitQuizAsync_ContinuesWhenNotificationFails()
        {

            var quiz = CreateTestQuiz();
            var quizzes = new List<Quiz> { quiz }.BuildMockDbSet().Object;
            _mockQuizRepo.Setup(r => r.GetQueryable()).Returns(quizzes);

            var dto = new SubmitQuizDTO
            {
                QuizId = quiz.Id,
                StudentId = "student1",
                Answers = quiz.Questions.Select(q => new StudentAnswerDTO
                {
                    QuestionId = q.Id,
                    SelectedAnswer = q.CorrectAnswer
                }).ToList()
            };

            SetupMockForSubmission();


            _mockNotificationService.Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDTO>()))
                .ThrowsAsync(new Exception("Notification service down"));


            var result = await _service.SubmitQuizAsync(dto);


            Assert.True(result.Success);
            Assert.Equal(100, result.Data.Grade);
        }

        #endregion

        #region Helper Methods

        private Quiz CreateTestQuiz(
            int questionCount = 2,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var quizId = Guid.NewGuid().ToString();
            var quiz = new Quiz
            {
                Id = quizId,
                Title = "Test Quiz",
                Description = "Test Description",
                DurationMinutes = 30,
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-1),
                EndDate = endDate ?? DateTime.UtcNow.AddDays(7),
                CourseId = "course1",
                InstructorId = "instructor1",
                NumberOfQuestions = questionCount,
                Questions = new List<Question>(),
                Course = new Course { Id = "course1", Name = "Test Course" }
            };

            for (int i = 0; i < questionCount; i++)
            {
                quiz.Questions.Add(new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    QuizId = quizId,
                    Text = $"Question {i + 1}",
                    OptionA = "Option A",
                    OptionB = "Option B",
                    OptionC = "Option C",
                    OptionD = "Option D",
                    CorrectAnswer = Options.OptionA,
                    Points = 10
                });
            }

            return quiz;
        }

        private Quiz CreateTestQuizWithDifferentPoints()
        {
            var quizId = Guid.NewGuid().ToString();
            var quiz = new Quiz
            {
                Id = quizId,
                Title = "Test Quiz",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                CourseId = "course1",
                InstructorId = "instructor1",
                NumberOfQuestions = 3,
                Questions = new List<Question>
                {
                    new Question { Id = "q1", QuizId = quizId, Text = "Q1", CorrectAnswer = Options.OptionA, Points = 5, OptionA = "A", OptionB = "B" },
                    new Question { Id = "q2", QuizId = quizId, Text = "Q2", CorrectAnswer = Options.OptionB, Points = 10, OptionA = "A", OptionB = "B" },
                    new Question { Id = "q3", QuizId = quizId, Text = "Q3", CorrectAnswer = Options.OptionC, Points = 15, OptionA = "A", OptionB = "B", OptionC = "C" }
                },
                Course = new Course { Id = "course1", Name = "Test Course" }
            };

            return quiz;
        }

        private Options GetWrongAnswer(Options correctAnswer)
        {
            return correctAnswer == Options.OptionA ? Options.OptionB : Options.OptionA;
        }

        private void SetupMockForSubmission()
        {
            var emptyStudentQuizzes = new List<StudentQuiz>().BuildMockDbSet().Object;
            _mockUnitOfWork.Setup(u => u.GetQueryable<StudentQuiz>()).Returns(emptyStudentQuizzes);

            var emptyStudentAnswers = new List<StudentAnswerQuestion>().BuildMockDbSet().Object;
            _mockUnitOfWork.Setup(u => u.GetQueryable<StudentAnswerQuestion>()).Returns(emptyStudentAnswers);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        }

        [Fact]
        public async Task GetStudentQuizStatusAsync_ReturnsBestAttempt_WhenMultipleAttemptsExist()
        {

            var quizId = "quiz1";
            var studentId = "student1";
            var attempts = new List<StudentQuiz>
            {
                new StudentQuiz { QuizId = quizId, StudentId = studentId, Grade = 50, Status = QuizStatus.Completed, StartTime = DateTime.Now.AddDays(-2) },
                new StudentQuiz { QuizId = quizId, StudentId = studentId, Grade = 80, Status = QuizStatus.Completed, StartTime = DateTime.Now.AddDays(-1) },
                new StudentQuiz { QuizId = quizId, StudentId = studentId, Grade = 60, Status = QuizStatus.Completed, StartTime = DateTime.Now }
            };

            var mockSet = attempts.AsQueryable().BuildMockDbSet();
            _mockUnitOfWork.Setup(u => u.GetQueryable<StudentQuiz>()).Returns(mockSet.Object);


            var result = await _service.GetStudentQuizStatusAsync(quizId, studentId);


            Assert.True(result.Success);
            Assert.Equal(80, result.Data.Grade);
        }

        #endregion
    }
}

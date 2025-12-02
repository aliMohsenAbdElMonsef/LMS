using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Notification;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class StudentEnrollmentServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IStudentEnrollIntoCourseRepository> _studentEnrollRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly StudentEnrollIntoCourseService _enrollmentService;

        public StudentEnrollmentServicesTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _studentEnrollRepoMock = new Mock<IStudentEnrollIntoCourseRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();

            _unitOfWorkMock.Setup(u => u.StudentEnrollments).Returns(_studentEnrollRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

            _enrollmentService = new StudentEnrollIntoCourseService(_unitOfWorkMock.Object, _emailServiceMock.Object);
        }

        [Fact]
        public async Task EnrollAsync_ShouldReturnFailure_WhenCourseNotFound()
        {
            // Arrange
            var dto = new RequestEnrollIntoCourseDTO { UserId = "student1", CourseId = "nonexistent" };
            var user = new ApplicationUser { Id = "student1" };

            _userRepoMock.Setup(r => r.FindByIdAsync("student1")).ReturnsAsync(user);
            _courseRepoMock.Setup(r => r.FindByIdAsync("nonexistent")).ReturnsAsync(null as Course);

            // Act
            var result = await _enrollmentService.EnrollAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Course not found.", result.Message);
        }

        [Fact]
        public async Task EnrollAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new RequestEnrollIntoCourseDTO { UserId = "nonexistent", CourseId = "course1" };
            _userRepoMock.Setup(r => r.FindByIdAsync("nonexistent")).ReturnsAsync(null as ApplicationUser);

            // Act
            var result = await _enrollmentService.EnrollAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }

        [Fact]
        public async Task ApproveEnrollment_ShouldSendNotification_WhenApproved()
        {
            // Arrange
            var dto = new UpdateStudentEnrollmentDTO { UserId = "student1", CourseId = "course1" };
            var enrollment = new StudentEnrollIntoCourse 
            { 
                StudentId = "student1", 
                CourseId = "course1", 
                Status = ApplicationStatus.Pending,
                Course = new Course { Name = "Test Course" },
                Student = new ApplicationUser { UserName = "student1", Email = "student@test.com" }
            };

            _studentEnrollRepoMock.Setup(r => r.GetFirstOrDefaultAsync("student1", "course1", "Student,Course")).ReturnsAsync(enrollment);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _enrollmentService.ApproveEnrollment(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Enrollment approved successfully.", result.Message);
            Assert.Equal(ApplicationStatus.Approved, enrollment.Status);
            _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetEnrollmentsAsync_ShouldReturnUserEnrollments()
        {
            // Arrange
            var userId = "student1";
            var enrollments = new List<StudentEnrollIntoCourse>
            {
                new StudentEnrollIntoCourse { StudentId = userId, CourseId = "course1", Student = new ApplicationUser { UserName = "student" }, Course = new Course { Name = "Course 1", CourseCode = "C1" } },
                new StudentEnrollIntoCourse { StudentId = userId, CourseId = "course2", Student = new ApplicationUser { UserName = "student" }, Course = new Course { Name = "Course 2", CourseCode = "C2" } }
            };

            _studentEnrollRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(enrollments);

            // Act
            var result = await _enrollmentService.GetEnrollmentsAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
        }
    }
}

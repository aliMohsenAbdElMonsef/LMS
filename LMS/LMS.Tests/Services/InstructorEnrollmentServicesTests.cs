using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class InstructorEnrollmentServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IInstructorEnrolltoCourseRepository> _instructorEnrollRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly InstructorEnrollIntoCourseService _enrollmentService;

        public InstructorEnrollmentServicesTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _instructorEnrollRepoMock = new Mock<IInstructorEnrolltoCourseRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _unitOfWorkMock.Setup(u => u.InstructorEnrollments).Returns(_instructorEnrollRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

            _enrollmentService = new InstructorEnrollIntoCourseService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task EnrollAsync_ShouldReturnSuccess_WhenValidRequest()
        {
            // Arrange
            var dto = new RequestEnrollIntoCourseDTO { UserId = "instructor1", CourseId = "course1" };
            var user = new ApplicationUser { Id = "instructor1" };
            var course = new Course { Id = "course1" };

            _userRepoMock.Setup(r => r.FindByIdAsync("instructor1")).ReturnsAsync(user);
            _courseRepoMock.Setup(r => r.FindByIdAsync("course1")).ReturnsAsync(course);
            _instructorEnrollRepoMock.Setup(r => r.GetByInstructorAndCourseAsync("instructor1", "course1")).ReturnsAsync(null as InstructorEnrolltoCourse);
            _instructorEnrollRepoMock.Setup(r => r.CreateAsync(It.IsAny<InstructorEnrolltoCourse>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _enrollmentService.EnrollAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Instructor enrollment request submitted successfully.", result.Message);
        }

        [Fact]
        public async Task EnrollAsync_ShouldReturnFailure_WhenInstructorNotFound()
        {
            // Arrange
            var dto = new RequestEnrollIntoCourseDTO { UserId = "nonexistent", CourseId = "course1" };
            _userRepoMock.Setup(r => r.FindByIdAsync("nonexistent")).ReturnsAsync(null as ApplicationUser);

            // Act
            var result = await _enrollmentService.EnrollAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Instructor not found.", result.Message);
        }

        [Fact]
        public async Task ApproveEnrollment_ShouldUpdateStatus_WhenEnrollmentExists()
        {
            // Arrange
            var dto = new UpdateEnrollIntoCourseDTO { UserId = "instructor1", CourseId = "course1" };
            var enrollment = new InstructorEnrolltoCourse 
            { 
                InstructorId = "instructor1", 
                CourseId = "course1", 
                Status = ApplicationStatus.Pending 
            };

            _instructorEnrollRepoMock.Setup(r => r.GetByInstructorAndCourseAsync("instructor1", "course1")).ReturnsAsync(enrollment);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _enrollmentService.ApproveEnrollment(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Instructor enrollment approved successfully.", result.Message);
            Assert.Equal(ApplicationStatus.Approved, enrollment.Status);
            Assert.NotNull(enrollment.ApprovedAt);
        }

        [Fact]
        public async Task DenyEnrollment_ShouldUpdateStatus_WhenEnrollmentExists()
        {
            // Arrange
            var dto = new UpdateEnrollIntoCourseDTO { UserId = "instructor1", CourseId = "course1" };
            var enrollment = new InstructorEnrolltoCourse 
            { 
                InstructorId = "instructor1", 
                CourseId = "course1", 
                Status = ApplicationStatus.Pending 
            };

            _instructorEnrollRepoMock.Setup(r => r.GetByInstructorAndCourseAsync("instructor1", "course1")).ReturnsAsync(enrollment);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _enrollmentService.DenyEnrollment(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Instructor enrollment denied.", result.Message);
            Assert.Equal(ApplicationStatus.Rejected, enrollment.Status);
        }

        [Fact]
        public async Task GetEnrollmentsAsync_ShouldReturnInstructorEnrollments()
        {
            // Arrange
            var instructorId = "instructor1";
            var enrollments = new List<InstructorEnrolltoCourse>
            {
                new InstructorEnrolltoCourse { InstructorId = instructorId, CourseId = "course1", Instructor = new ApplicationUser { UserName = "instructor" }, Course = new Course { Name = "Course 1", CourseCode = "C1" } }
            };

            _instructorEnrollRepoMock.Setup(r => r.GetByInstructorIdAsync(instructorId)).ReturnsAsync(enrollments);

            // Act
            var result = await _enrollmentService.GetEnrollmentsAsync(instructorId);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
        }
    }
}

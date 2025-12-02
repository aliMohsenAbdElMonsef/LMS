using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Enums;
using Moq;
using LMS.BusinessLogic.Contracts.Services;
using Xunit;

namespace LMS.Tests.Services
{
    public class AssignmentServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IAssignmentRepository> _assignmentRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IStudentEnrollIntoCourseRepository> _studentEnrollmentRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AssignmentServices _assignmentServices;

        public AssignmentServicesTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _assignmentRepoMock = new Mock<IAssignmentRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _studentEnrollmentRepoMock = new Mock<IStudentEnrollIntoCourseRepository>();

            _unitOfWorkMock.Setup(u => u.Assignments).Returns(_assignmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.StudentEnrollments).Returns(_studentEnrollmentRepoMock.Object);

            _assignmentServices = new AssignmentServices(_unitOfWorkMock.Object, _mapperMock.Object, _emailServiceMock.Object);
        }

        [Fact]
        public async Task SubmitAssignmentAsync_ShouldReturnSuccess_WhenNewSubmission()
        {
            // Arrange
            var dto = new SubmitAssignmentDTO { AssignmentId = "assign1", StudentId = "student1", FilePath = "path/to/file" };
            var assignment = new Assignment { Id = "assign1", DueDate = DateTime.UtcNow.AddDays(1) };
            var student = new ApplicationUser { Id = "student1" };
            var submissionDto = new StudentAssignmentDTO { Id = "sub1", Status = AssignmentStatus.PendingGrading };

            _assignmentRepoMock.Setup(r => r.FindByIdAsync("assign1")).ReturnsAsync(assignment);
            _userRepoMock.Setup(r => r.FindByIdAsync("student1")).ReturnsAsync(student);
            _assignmentRepoMock.Setup(r => r.GetStudentAssignmentAsync("assign1", "student1")).ReturnsAsync((StudentAssignment?)null);
            _assignmentRepoMock.Setup(r => r.CreateStudentAssignmentAsync(It.IsAny<StudentAssignment>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            
            // Mock getting the created submission
            var createdSubmission = new StudentAssignment { Id = "sub1", Status = AssignmentStatus.PendingGrading };
            _assignmentRepoMock.Setup(r => r.GetStudentAssignmentAsync("assign1", "student1")).ReturnsAsync(createdSubmission);
            _mapperMock.Setup(m => m.Map<StudentAssignmentDTO>(createdSubmission)).Returns(submissionDto);

            // Act
            var result = await _assignmentServices.SubmitAssignmentAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Assignment submitted successfully", result.Message);
            Assert.Equal(AssignmentStatus.PendingGrading, result.Data.Status);
        }

        [Fact]
        public async Task GradeAssignmentAsync_ShouldReturnSuccess_WhenSubmissionExists()
        {
            // Arrange
            var dto = new GradeAssignmentDTO { StudentAssignmentId = "sub1", Grade = 95, Feedback = "Good job" };
            var submission = new StudentAssignment { Id = "sub1", Status = AssignmentStatus.PendingGrading };
            var gradedDto = new StudentAssignmentDTO { Id = "sub1", Grade = 95, Status = AssignmentStatus.Graded };

            _assignmentRepoMock.Setup(r => r.GetStudentAssignmentByIdAsync("sub1")).ReturnsAsync(submission);
            _assignmentRepoMock.Setup(r => r.UpdateStudentAssignmentAsync(submission)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<StudentAssignmentDTO>(submission)).Returns(gradedDto);

            // Act
            var result = await _assignmentServices.GradeAssignmentAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Assignment graded successfully", result.Message);
            Assert.Equal(95, submission.Grade);
            Assert.Equal(AssignmentStatus.Graded, submission.Status);
        }

        [Fact]
        public async Task GetStudentAllAssignmentsAsync_ShouldReturnAssignments_WhenEnrolled()
        {
            // Arrange
            var studentId = "student1";
            var assignment = new Assignment { Id = "assign1", CourseId = "course1", Title = "Test Assignment" };
            var course = new Course { Id = "course1", Name = "Test Course" };
            
            _assignmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Assignment> { assignment });
            _studentEnrollmentRepoMock.Setup(r => r.IsStudentEnrolledInCourseAsync(studentId, "course1")).ReturnsAsync(true);
            _courseRepoMock.Setup(r => r.FindByIdAsync("course1")).ReturnsAsync(course);
            _assignmentRepoMock.Setup(r => r.GetStudentAssignmentAsync("assign1", studentId)).ReturnsAsync((StudentAssignment?)null);

            // Act
            var result = await _assignmentServices.GetStudentAllAssignmentsAsync(studentId);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
            Assert.Equal("Test Assignment", result.Data.First().AssignmentTitle);
        }
    }
}

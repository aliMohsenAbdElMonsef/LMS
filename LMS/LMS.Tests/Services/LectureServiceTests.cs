using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;
using MockQueryable.Moq;
using Moq;
using LMS.BusinessLogic.Contracts.Services;
using Xunit;

namespace LMS.Tests.Services
{
    public class LectureServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILectureRepository> _lectureRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<IStudentEnrollIntoCourseRepository> _studentEnrollmentRepoMock;
        private readonly Mock<IInstructorEnrolltoCourseRepository> _instructorEnrollmentRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly LectureService _lectureService;

        public LectureServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _fileServiceMock = new Mock<IFileService>();
            _lectureRepoMock = new Mock<ILectureRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _studentEnrollmentRepoMock = new Mock<IStudentEnrollIntoCourseRepository>();
            _instructorEnrollmentRepoMock = new Mock<IInstructorEnrolltoCourseRepository>();

            _unitOfWorkMock.Setup(u => u.Lectures).Returns(_lectureRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.StudentEnrollments).Returns(_studentEnrollmentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.InstructorEnrollments).Returns(_instructorEnrollmentRepoMock.Object);

            _lectureService = new LectureService(_unitOfWorkMock.Object, _mapperMock.Object, _emailServiceMock.Object, _fileServiceMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenAdminCreatesLecture()
        {

            var dto = new CreateLectureDTO
            {
                CourseId = "course1",
                Title = "Test Lecture",
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(11, 0, 0)
            };
            var userId = "admin1";
            var userRole = "Admin";
            var lecture = new Lecture { Id = "lecture1" };
            var getLectureDto = new GetLectureDTO { Id = "lecture1" };

            _mapperMock.Setup(m => m.Map<Lecture>(dto)).Returns(lecture);
            _lectureRepoMock.Setup(r => r.CreateAsync(lecture)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _lectureRepoMock.Setup(r => r.FindByIdAsync("lecture1")).ReturnsAsync(lecture);
            _mapperMock.Setup(m => m.Map<GetLectureDTO>(lecture)).Returns(getLectureDto);


            var result = await _lectureService.CreateAsync(dto, userId, userRole);



            Assert.True(result.Success);
            Assert.Equal("Lecture created successfully", result.Message);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnError_WhenInstructorNotEnrolled()
        {

            var dto = new CreateLectureDTO { CourseId = "course1" };
            var userId = "instructor1";
            var userRole = "Instructor";
            var course = new Course { Id = "course1" };

            _courseRepoMock.Setup(r => r.FindByIdAsync("course1")).ReturnsAsync(course);
            _instructorEnrollmentRepoMock.Setup(r => r.GetByInstructorAndCourseAsync(userId, "course1"))
                .ReturnsAsync(null as InstructorEnrolltoCourse);


            var result = await _lectureService.CreateAsync(dto, userId, userRole);


            Assert.False(result.Success);
            Assert.Equal("You are not authorized to create lectures for this course", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnLecture_WhenStudentIsEnrolled()
        {

            var courseId = "course1";
            var lectureId = "lecture1";
            var userId = "student1";
            var userRole = "Student";
            var course = new Course { Id = courseId };
            var lecture = new Lecture { Id = lectureId, CourseId = courseId };
            var enrollment = new StudentEnrollIntoCourse { StudentId = userId, CourseId = courseId, Status = ApplicationStatus.Approved };
            var getLectureDto = new GetLectureDTO { Id = lectureId };

            _courseRepoMock.Setup(r => r.FindByIdAsync(courseId)).ReturnsAsync(course);
            _lectureRepoMock.Setup(r => r.GetCourseLecturesAsync(courseId)).ReturnsAsync(new List<Lecture> { lecture });
            _lectureRepoMock.Setup(r => r.FindByIdAsync(lectureId)).ReturnsAsync(lecture);
            _studentEnrollmentRepoMock.Setup(r => r.GetFirstOrDefaultAsync(userId, courseId, It.IsAny<string?>())).ReturnsAsync(enrollment);
            _mapperMock.Setup(m => m.Map<GetLectureDTO>(lecture)).Returns(getLectureDto);


            var result = await _lectureService.GetByIdAsync(courseId, lectureId, userId, userRole);


            Assert.True(result.Success);
            Assert.Equal(lectureId, result.Data.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenStudentNotEnrolled()
        {

            var courseId = "course1";
            var lectureId = "lecture1";
            var userId = "student1";
            var userRole = "Student";
            var course = new Course { Id = courseId };
            var lecture = new Lecture { Id = lectureId, CourseId = courseId };

            _courseRepoMock.Setup(r => r.FindByIdAsync(courseId)).ReturnsAsync(course);
            _lectureRepoMock.Setup(r => r.GetCourseLecturesAsync(courseId)).ReturnsAsync(new List<Lecture> { lecture });
            _lectureRepoMock.Setup(r => r.FindByIdAsync(lectureId)).ReturnsAsync(lecture);
            _studentEnrollmentRepoMock.Setup(r => r.GetFirstOrDefaultAsync(userId, courseId, It.IsAny<string?>())).ReturnsAsync(null as StudentEnrollIntoCourse);


            var result = await _lectureService.GetByIdAsync(courseId, lectureId, userId, userRole);


            Assert.False(result.Success);
            Assert.Equal("You are not authorized to access this lecture", result.Message);
        }
    }
}

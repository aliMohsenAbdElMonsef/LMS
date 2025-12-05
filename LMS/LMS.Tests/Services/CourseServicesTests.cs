using AutoMapper;
using LMS.BusinessLogic.Contracts.Services;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class CourseServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly CourseServices _courseServices;

        public CourseServicesTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _fileServiceMock = new Mock<IFileService>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>().Object);

            _courseServices = new CourseServices(_unitOfWorkMock.Object, _mapperMock.Object, _fileServiceMock.Object);
        }

        [Fact]
        public async Task CreateCourse_ShouldReturnSuccess_WhenDataIsValid()
        {

            var dto = new CreateCourseDTO
            {
                Name = "Test Course",
                CourseCode = "TC101",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(30),
                AdminId = "admin1",
                CategoryId = "cat1",
                IsFree = true
            };

            var admin = new ApplicationUser { Id = "admin1" };
            var category = new Category { Id = "cat1" };
            var course = new Course { Id = "course1", Name = "Test Course" };
            var getCourseDto = new GetCourseDTO { Id = "course1", Name = "Test Course" };

            _userRepoMock.Setup(r => r.FindByIdAsync("admin1")).ReturnsAsync(admin);
            _categoryRepoMock.Setup(r => r.FindByIdAsync("cat1")).ReturnsAsync(category);
            _courseRepoMock.Setup(r => r.FindByCodeAsync("TC101")).ReturnsAsync((Course?)null);
            _mapperMock.Setup(m => m.Map<Course>(dto)).Returns(course);
            _courseRepoMock.Setup(r => r.CreateAsync(course)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _courseRepoMock.Setup(r => r.FindByIdAsync("course1")).ReturnsAsync(course);
            _mapperMock.Setup(m => m.Map<GetCourseDTO>(course)).Returns(getCourseDto);


            var result = await _courseServices.CreateCourse(dto);


            Assert.True(result.Success);
            Assert.Equal("Course created successfully.", result.Message);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task CreateCourse_ShouldReturnFailure_WhenStartDateIsInPast()
        {

            var dto = new CreateCourseDTO
            {
                Name = "Test Course",
                CourseCode = "TC101",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30)
            };


            var result = await _courseServices.CreateCourse(dto);


            Assert.False(result.Success);
            Assert.Equal("The start date must be in the future.", result.Message);
        }

        [Fact]
        public async Task CreateCourse_ShouldReturnFailure_WhenEndDateIsBeforeStartDate()
        {

            var dto = new CreateCourseDTO
            {
                Name = "Test Course",
                CourseCode = "TC101",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(5)
            };


            var result = await _courseServices.CreateCourse(dto);


            Assert.False(result.Success);
            Assert.Equal("The start date must be before the end date.", result.Message);
        }

        [Fact]
        public async Task CreateCourse_ShouldReturnFailure_WhenCourseCodeExists()
        {

            var dto = new CreateCourseDTO
            {
                Name = "Test Course",
                CourseCode = "TC101",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(30),
                AdminId = "admin1",
                CategoryId = "cat1"
            };

            var admin = new ApplicationUser { Id = "admin1" };
            var category = new Category { Id = "cat1" };
            var existingCourse = new Course { Id = "existing", CourseCode = "TC101" };

            _userRepoMock.Setup(r => r.FindByIdAsync("admin1")).ReturnsAsync(admin);
            _categoryRepoMock.Setup(r => r.FindByIdAsync("cat1")).ReturnsAsync(category);
            _courseRepoMock.Setup(r => r.FindByCodeAsync("TC101")).ReturnsAsync(existingCourse);


            var result = await _courseServices.CreateCourse(dto);


            Assert.False(result.Success);
            Assert.Equal("A course with this code already exists.", result.Message);
        }
    }
}

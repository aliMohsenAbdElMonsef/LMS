using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class CategoryServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly CategoryServices _categoryServices;

        public CategoryServicesTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();

            _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);

            _categoryServices = new CategoryServices(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetCategoryWithCourseDetails_ShouldReturnCategory_WhenExists()
        {

            var categoryId = "cat1";
            var category = new Category { Id = categoryId, Name = "Test Category" };
            var courses = new List<Course> { new Course { Id = "c1", Name = "Course 1" } };
            var courseDtos = new List<GetCourseDTO> { new GetCourseDTO { Id = "c1", Name = "Course 1" } };

            _categoryRepoMock.Setup(r => r.FindByIdAsync(categoryId)).ReturnsAsync(category);
            _courseRepoMock.Setup(r => r.GetCoursesByCategoryIdAsync(categoryId)).ReturnsAsync(courses);
            _mapperMock.Setup(m => m.Map<List<GetCourseDTO>>(courses)).Returns(courseDtos);


            var result = await _categoryServices.GetCategoryWithCourseDetails(categoryId);


            Assert.True(result.Success);
            Assert.Equal(categoryId, result.Data.Id);
            Assert.Equal(1, result.Data.CoursesCount);
        }

        [Fact]
        public async Task GetCategoryWithCourseDetails_ShouldReturnError_WhenCategoryNotFound()
        {

            var categoryId = "nonexistent";
            _categoryRepoMock.Setup(r => r.FindByIdAsync(categoryId)).ReturnsAsync((Category?)null);


            var result = await _categoryServices.GetCategoryWithCourseDetails(categoryId);


            Assert.False(result.Success);
            Assert.Equal("Category not found.", result.Message);
        }

        [Fact]
        public async Task GetCategoryAsync_ShouldReturnCategory_WhenExists()
        {

            var categoryId = "cat1";
            var category = new Category { Id = categoryId, Name = "Test Category" };
            var dto = new ReadCategoryDTO { Id = categoryId, Name = "Test Category" };

            _categoryRepoMock.Setup(r => r.FindByIdAsync(categoryId)).ReturnsAsync(category);
            _mapperMock.Setup(m => m.Map<ReadCategoryDTO>(category)).Returns(dto);


            var result = await _categoryServices.GetCategoryAsync(categoryId);


            Assert.True(result.Success);
            Assert.Equal(categoryId, result.Data.Id);
        }
    }
}

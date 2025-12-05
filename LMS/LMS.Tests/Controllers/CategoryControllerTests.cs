using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Recieve.Categories;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<IUnitOfServices> _mockUnitOfServices;
        private readonly Mock<ICategoryServices> _mockCategoryServices;
        private readonly Mock<IUserServices> _mockUserServices;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockUnitOfServices = new Mock<IUnitOfServices>();
            _mockCategoryServices = new Mock<ICategoryServices>();
            _mockUserServices = new Mock<IUserServices>();

            _mockUnitOfServices.Setup(u => u.Categories).Returns(_mockCategoryServices.Object);
            _mockUnitOfServices.Setup(u => u.Users).Returns(_mockUserServices.Object);

            _controller = new CategoryController(_mockUnitOfServices.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-id"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateCategory_ReturnsOk_WhenSuccessful()
        {

            var createDto = new ReadCategoryFromUserDTO { Name = "Test Category", Description = "Desc" };
            var responseDto = new ServiceResponseDTO<ReadCategoryDTO> { Success = true };

            _mockUserServices.Setup(s => s.GetUserName("admin-id")).ReturnsAsync("Admin User");
            _mockCategoryServices.Setup(s => s.CreateAsync(It.IsAny<CreateCategoryDTO>()))
                .ReturnsAsync(responseDto);


            var result = await _controller.CreateCategory(createDto);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(((ServiceResponseDTO<ReadCategoryDTO>)okResult.Value).Success);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsOk_WhenSuccessful()
        {

            var updateDto = new UpdateCategoryDTO { Id = "1", Name = "Updated" };
            var responseDto = new ServiceResponseDTO<ReadCategoryDTO> { Success = true };

            _mockCategoryServices.Setup(s => s.UpdateAsync(updateDto))
                .ReturnsAsync(responseDto);


            var result = await _controller.UpdateCategory(updateDto);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {

            var responseDto = new ServiceResponseDTO<IEnumerable<ReadCategoryDTO>> { Success = true, Data = new List<ReadCategoryDTO>() };

            _mockCategoryServices.Setup(s => s.GetAllAsync())
                .ReturnsAsync(responseDto);


            var result = await _controller.GetAllCategories();


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCategoryByIdInDetails_ReturnsOk_WhenFound()
        {

            var id = "1";
            var responseDto = new ServiceResponseDTO<CategoryDetailsDTO> { Success = true };

            _mockCategoryServices.Setup(s => s.GetCategoryWithCourseDetails(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetCategoryByIdInDetails(id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk_WhenFound()
        {

            var id = "1";
            var responseDto = new ServiceResponseDTO<ReadCategoryDTO> { Success = true };

            _mockCategoryServices.Setup(s => s.GetCategoryAsync(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetCategoryById(id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

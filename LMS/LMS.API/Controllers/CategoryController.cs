using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;

        public CategoryController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }

        private ICategoryServices CategoryService => _unitOfServices.Categories;

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCategoryDTO>>> CreateCategory(CreateCategoryDTO category)
        {
            var result = await CategoryService.CreateAsync(category);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCategoryDTO>>> UpdateCategory(UpdateCategoryDTO category)
        {
            var result = await CategoryService.UpdateAsync(category);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<List<ReadCategoryDTO>>>> GetAllCategories()
        {
            var result = await CategoryService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<ReadCategoryDTO>>> GetCategoryById(string id)
        {
            var result = await CategoryService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
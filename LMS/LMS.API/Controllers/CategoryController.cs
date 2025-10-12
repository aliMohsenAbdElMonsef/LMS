using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Category;
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
        private ICategoryServices CategoryService => _unitOfServices.Categories ;

        [HttpPost]
        public ActionResult CreateCategory(CreateCategoryDTO category)
        {
            return Ok( CategoryService.Create(category) );   
        }
    }
}

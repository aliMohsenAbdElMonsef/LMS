using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICategoryServices : IBaseService<ReadCategoryDTO, CreateCategoryDTO, UpdateCategoryDTO>
    {
        Task<ServiceResponseDTO<CategoryDetailsDTO>> GetCategoryWithCourseDetails(string Id);
        Task<ServiceResponseDTO<ReadCategoryDTO>> GetCategoryAsync(string Id);
        Task<ServiceResponseDTO<List<ReadCategoryDTO>>> GetTopCategoriesAsync(int count);
    }
}

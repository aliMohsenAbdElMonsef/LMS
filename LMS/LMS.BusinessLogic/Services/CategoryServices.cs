using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CategoryServices : BaseServices<Category, ReadCategoryDTO, CreateCategoryDTO, UpdateCategoryDTO>, ICategoryServices
    {
        private readonly IMapper _mapper;
        public CategoryServices(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }
        protected CategoryDetailsDTO MapToCategoryDetailsDTO(List<Course> courses, Category category)
        {
            List<GetCourseDTO> dtoCourses = _mapper.Map<List<GetCourseDTO>>(courses);
            CategoryDetailsDTO dto = new CategoryDetailsDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreationDate = category.CreationDate,
                LastUpdated = category.LastUpdated,
                CoursesCount = courses.Count,
                Courses = dtoCourses,
            };
            return dto;
        }
        public async Task<ServiceResponseDTO<CategoryDetailsDTO>> GetCategoryWithCourseDetails(string Id)
        {
            Category? category = await _unitOfWork.Categories.FindByIdAsync(Id);
            if (category == null)
            {
                ServiceResponseDTO<CategoryDetailsDTO> Errorresponse = new ServiceResponseDTO<CategoryDetailsDTO>
                {
                    Success = false,
                    Message = "Category not found."
                };
                return Errorresponse;
            }
            List<Course> courses = await _unitOfWork.Courses.GetCoursesByCategoryIdAsync(category.Id);
            if (courses == null)
            {
                courses = new List<Course>();
            }
            CategoryDetailsDTO dto = MapToCategoryDetailsDTO(courses, category);

            ServiceResponseDTO<CategoryDetailsDTO> response = new ServiceResponseDTO<CategoryDetailsDTO>
            {
                Success = true,
                Message = "operation passed successfully",
                Data = dto

            };
            return response;

        }
        public async Task<ServiceResponseDTO<ReadCategoryDTO>> GetCategoryAsync(string Id)
        {
            Category? category = await _unitOfWork.Categories.FindByIdAsync(Id);
            if (category == null)
            {
                ServiceResponseDTO<ReadCategoryDTO> Errorresponse = new ServiceResponseDTO<ReadCategoryDTO>
                {
                    Success = false,
                    Message = "Category not found."
                };
                return Errorresponse;
            }
            var dto = _mapper.Map<ReadCategoryDTO>(category);
            ServiceResponseDTO<ReadCategoryDTO> responce = new ServiceResponseDTO<ReadCategoryDTO>
            {
                Success = true,
                Message = "Operation passed successfully.",
                Data = dto
            };
            return responce;

        }

        protected override string GetIdFromUpdateDTO(UpdateCategoryDTO dto) => dto.Id;

        protected override IBaseRepository<Category, string> GetRepo() => _unitOfWork.Categories;

        protected override Category MapToEntity(CreateCategoryDTO dto)
        {
            Category category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Description = dto.Description,
                AdminId = dto.AdminID,
                CreationDate = DateTime.UtcNow,
            };
            category.LastUpdated = category.CreationDate;
            category.Admin = _unitOfWork.Users.FindByIdAsync(dto.AdminID).Result;
            return category;
        }

        protected override ReadCategoryDTO MapToReadDTO(Category entity)
        {

            return new ReadCategoryDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreationDate = entity.CreationDate,
                LastUpdated = entity.LastUpdated,
                AdminId = entity.AdminId,
                AdminName = entity.Admin != null ? $"{entity.Admin.FirstName} {entity.Admin.LastName}" : null,
                CoursesCount = entity.Courses?.Count(c => c != null && !c.IsDeleted) ?? 0
            };
        }

        protected override Category UpdateToEntity(UpdateCategoryDTO dto, Category existingEntity)
        {
            existingEntity.Name = dto.Name ?? existingEntity.Name;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.LastUpdated = DateTime.UtcNow;
            return existingEntity;
            existingEntity.LastUpdated = DateTime.UtcNow;
            return existingEntity;
        }

        public async Task<ServiceResponseDTO<List<ReadCategoryDTO>>> GetTopCategoriesAsync(int count)
        {
            var response = new ServiceResponseDTO<List<ReadCategoryDTO>>();
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var topCategories = categories
                    .OrderByDescending(c => c.Courses?.Count(course => !course.IsDeleted) ?? 0)
                    .Take(count)
                    .ToList();

                var dtos = _mapper.Map<List<ReadCategoryDTO>>(topCategories);
                
                // Manually populate CoursesCount since it might not be mapped automatically depending on configuration
                for (int i = 0; i < topCategories.Count; i++)
                {
                    dtos[i].CoursesCount = topCategories[i].Courses?.Count(c => !c.IsDeleted) ?? 0;
                }

                response.Success = true;
                response.Data = dtos;
                response.Message = "Top categories retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving top categories: {ex.Message}";
            }
            return response;
        }
    }
     
}

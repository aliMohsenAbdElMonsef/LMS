using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Category;
using LMS.DataAccess.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CategoryServices : BaseServices<Category, ReadCategoryDTO, CreateCategoryDTO, UpdateCategoryDTO>,ICategoryServices
    {
        public CategoryServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
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
                AdminId = dto.AdminId,
                CreationDate = DateTime.UtcNow,
            };
            category.LastUpdated = category.CreationDate;
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
                CoursesCount = entity.Courses?.Count(c => c!=null && !c.IsDeleted)?? 0
            };
        }

        protected override Category UpdateToEntity(UpdateCategoryDTO dto, Category existingEntity)
        {
            existingEntity.Name = dto.Name?? existingEntity.Name;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.LastUpdated = DateTime.UtcNow;
            return existingEntity;
        }
    }
}

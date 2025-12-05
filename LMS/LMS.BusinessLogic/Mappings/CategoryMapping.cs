using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Course;

namespace LMS.BusinessLogic.Mappings
{
    internal class CategoryMapping : Profile
    {
        public CategoryMapping()
        {
            CreateMap<CreateCategoryDTO, Category>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Category, ReadCategoryDTO>()
                .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.Admin.UserName))
                .ForMember(dest => dest.CoursesCount, opt => opt.MapFrom(src => src.Courses.Count));

            CreateMap<UpdateCategoryDTO, Category>();
        }
    }
}

using AutoMapper;
using LMS.BusinessLogic.DTOs.Category;
using LMS.MVC.Models.ViewModels.Category;

namespace LMS.MVC.Mappers
{
    internal class CategoryMapping : Profile
    {
        public CategoryMapping()
        {
            CreateMap<ReadCategoryDTO, ReadCategoryResult>();
            CreateMap<CategoryDetailsDTO, CategoryDetailsResult>();

            CreateMap<ReadCategoryResult, CreateCategoryDTO>()
                .ForMember(dest => dest.AdminID, opt => opt.MapFrom(src => src.AdminId)) 
                .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.AdminName));

            CreateMap<ReadCategoryResult, UpdateCategoryDTO>()
                .ForMember(dest => dest.AdminID, opt => opt.MapFrom(src => src.AdminId))
                .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.AdminName));

        }
    }
}

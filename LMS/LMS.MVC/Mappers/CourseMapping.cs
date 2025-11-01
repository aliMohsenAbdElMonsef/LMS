using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Course;
using LMS.MVC.Models.ViewModels.Course;

namespace LMS.MVC.Mappers
{
    internal class CourseMapping : Profile
    {
        public CourseMapping()
        {
            CreateMap<GetCourseDTO, ReadCourseResult>();
        }
    }

}

using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMS.BusinessLogic.DTOs.Assignment;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Course;

namespace LMS.BusinessLogic.Mappings
{
    internal class CourseMapping : Profile
    {
        public CourseMapping()
        {
            
            CreateMap<Course, GetCourseDTO>()
                .ForMember(dest => dest.Quizzes, opt => opt.Ignore());
 
            CreateMap<CreateCourseDTO, Course>();

            CreateMap<UpdateCourseDTO, Course>();
        }

    }
}

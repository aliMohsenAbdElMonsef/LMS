using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.LectureSchedule;

namespace LMS.BusinessLogic.Mappings
{
    public class LectureScheduleMapping : Profile
    {
        public LectureScheduleMapping()
        {
            CreateMap<LectureSchedule, GetLectureScheduleDTO>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : null))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? $"{src.Instructor.FirstName} {src.Instructor.LastName}" : "Unknown"))
                .ForMember(dest => dest.GeneratedLecturesCount, opt => opt.Ignore());

            CreateMap<DayScheduleDTO, LectureSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Lectures, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore());

            CreateMap<UpdateLectureScheduleDTO, LectureSchedule>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Lectures, opt => opt.Ignore());
        }
    }
}

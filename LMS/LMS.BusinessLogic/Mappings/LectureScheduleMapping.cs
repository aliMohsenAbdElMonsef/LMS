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
                .ForMember(dest => dest.GeneratedLecturesCount, opt => opt.Ignore()); // Manually mapped in service

            CreateMap<DayScheduleDTO, LectureSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.Ignore()) // Set manually
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Lectures, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore()); // InstructorId mapped by convention

            CreateMap<UpdateLectureScheduleDTO, LectureSchedule>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Lectures, opt => opt.Ignore());
        }
    }
}

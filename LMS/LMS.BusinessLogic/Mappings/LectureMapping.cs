using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Lecture;

namespace LMS.BusinessLogic.Mappings
{
    public class LectureMapping : Profile
    {
        public LectureMapping()
        {
            CreateMap<CreateLectureDTO, Lecture>()
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => CalculateEndTime(src)))
                .ForMember(dest => dest.LectureNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastRecordingUploadDate, opt => opt.Ignore())
                .ForMember(dest => dest.ZoomLink, opt => opt.Ignore())
                .ForMember(dest => dest.LectureScheduleId, opt => opt.Ignore())
                .ForMember(dest => dest.LastUploadedByInstructorId, opt => opt.Ignore());
            
            CreateMap<UpdateLectureDTO, Lecture>();
            
            CreateMap<Lecture, GetLectureDTO>()
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => 
                    (int)((src.EndTime < src.StartTime ? src.EndTime.Add(TimeSpan.FromDays(1)) : src.EndTime) - src.StartTime).TotalMinutes))
                .ForMember(dest => dest.LectureNumber, opt => opt.MapFrom(src => src.LectureNumber))
                .ForMember(dest => dest.Instructor, opt => opt.Ignore());
            
            CreateMap<Lecture, ReadLectureDTO>()
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => 
                    (int)((src.EndTime < src.StartTime ? src.EndTime.Add(TimeSpan.FromDays(1)) : src.EndTime) - src.StartTime).TotalMinutes));
        }

        private static TimeSpan CalculateEndTime(CreateLectureDTO src)
        {
            var end = src.EndTime ?? (src.DurationMinutes.HasValue 
                ? src.StartTime.Add(TimeSpan.FromMinutes(src.DurationMinutes.Value))
                : src.StartTime.Add(TimeSpan.FromHours(1)));
            
            if (end.TotalDays >= 1)
            {
                return end.Subtract(TimeSpan.FromDays((int)end.TotalDays));
            }
            return end;
        }
    }
}

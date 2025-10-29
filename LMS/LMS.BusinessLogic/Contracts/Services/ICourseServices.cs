using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.DaySchedule;
using LMS.BusinessLogic.DTOs.Lecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICourseServices : IBaseService<GetCourseDTO, CreateCourseDTO, UpdateCourseDTO>
    {   
        Task<IEnumerable<GetLectureDTO>> GetCourseLecturesAsync(string courseId);
        
        Task<GetCourseDTO> CreateCourseWithScheduleAsync(CreateCourseDTO dto);
       
        Task<IEnumerable<GetDayScheduleDTO>> GetCourseScheduleAsync(string courseId);
    }
}

using LMS.BusinessLogic.DTOs.DaySchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICourseDayScheduleServices : IBaseService<GetDayScheduleDTO, CreateDayScheduleDTO, UpdateDayScheduleDTO>
    {
        Task<IEnumerable<GetDayScheduleDTO>> GetCourseSchedulesAsync(string courseId);
    }
}

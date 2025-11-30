using LMS.BusinessLogic.DTOs.LectureSchedule;
using LMS.BusinessLogic.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ILectureScheduleService
    {
        Task<ServiceResponseDTO<GetLectureScheduleDTO>> CreateScheduleAsync(CreateLectureScheduleDTO dto, string userId, string userRole);
        Task<ServiceResponseDTO<GetLectureScheduleDTO>> UpdateScheduleAsync(UpdateLectureScheduleDTO dto, string userId, string userRole);
        Task<ServiceResponseDTO<GetLectureScheduleDTO>> GetScheduleByIdAsync(string id, string userId, string userRole);
        Task<ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>>> GetCourseSchedulesAsync(string courseId, string userId, string userRole);
        Task<ServiceResponseDTO<bool>> DeleteScheduleAsync(string id, string userId, string userRole);
        Task<ServiceResponseDTO<bool>> GenerateLecturesFromScheduleAsync(string scheduleId, string userId, string userRole);
    }
}

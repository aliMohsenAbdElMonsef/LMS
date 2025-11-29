using LMS.BusinessLogic.DTOs.LectureSchedule;
using LMS.BusinessLogic.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.MVC.Services.Contracts
{
    public interface ILectureScheduleService
    {
        Task<ServiceResponseDTO<GetLectureScheduleDTO>> CreateScheduleAsync(CreateLectureScheduleDTO dto);
        Task<ServiceResponseDTO<GetLectureScheduleDTO>> UpdateScheduleAsync(UpdateLectureScheduleDTO dto);
        Task<ServiceResponseDTO<GetLectureScheduleDTO>> GetScheduleByIdAsync(string id);
        Task<ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>>> GetCourseSchedulesAsync(string courseId);
        Task<ServiceResponseDTO<bool>> DeleteScheduleAsync(string id);
        Task<ServiceResponseDTO<bool>> GenerateLecturesFromScheduleAsync(string scheduleId);
    }
}

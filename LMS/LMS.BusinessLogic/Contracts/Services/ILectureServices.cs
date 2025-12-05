using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ILectureServices: IBaseService<GetLectureDTO, CreateLectureDTO, UpdateLectureDTO>
    {
        Task<ServiceResponseDTO<GetLectureDTO>> GetByIdAsync(string courseid, string lectureid, string userId, string userRole);
        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetCourseLecturesAsync(string courseId, string userId, string userRole);
        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetMyLecturesAsync(string userId, string userRole);
        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetInstructorLecturesAsync(string instructorId, string userId, string userRole);
        Task<ServiceResponseDTO<bool>> LaunchLectureAsync(string lectureId, string zoomLink, string userId, string userRole);

        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetUpcomingLecturesAsync(string courseId, string userId, string userRole);
        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetTodayLecturesAsync(string courseId, string userId, string userRole);
        Task<ServiceResponseDTO<GetLectureDTO>> UpdateAsync(UpdateLectureDTO dto, string userId, string userRole);
        Task DeleteAsync(string id, string userId, string userRole);
        Task<ServiceResponseDTO<GetLectureDTO>> CreateAsync(CreateLectureDTO dto, string userId, string userRole);
        Task<ServiceResponseDTO<bool>> CheckLectureConflictAsync(string courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, string excludeLectureId = null);
        Task<ServiceResponseDTO<bool>> MarkAsAttendedAsync(string lectureId, string studentId);
        Task<ServiceResponseDTO<AttendanceStatisticsDTO>> GetAttendanceStatisticsAsync(string studentId);
        Task<ServiceResponseDTO<AttendanceStatisticsDTO>> GetCourseAttendanceStatisticsAsync(string courseId, string studentId);
        Task<ServiceResponseDTO<GetLectureDTO>> UploadLectureContentAsync(string lectureId, IFormFile? recording, IFormFile? materials, string userId, string userRole);
    }
}

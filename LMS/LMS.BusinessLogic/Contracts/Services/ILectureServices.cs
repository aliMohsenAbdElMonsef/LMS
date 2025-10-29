using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ILectureServices: IBaseService<GetLectureDTO, CreateLectureDTO, UpdateLectureDTO>
    {
        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetCourseLecturesAsync(string courseId, string userId, string userRole);
        Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetInstructorLecturesAsync(string instructorId, string userId, string userRole);

        Task<ServiceResponseDTO<bool>> CheckLectureConflictAsync(string courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, string excludeLectureId = null);
    }
}

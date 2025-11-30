using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.DaySchedule;
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
    public interface ICourseServices : IBaseService<GetCourseDTO, CreateCourseDTO, UpdateCourseDTO>
    {
        Task<ServiceResponseDTO<GetCourseDTO>> UpdateCourseWithThumbnail(UpdateCourseDTO dto, IFormFile thumbnailFile);
        Task<ServiceResponseDTO<GetCourseDTO>> UpdateThumbnailAsync(string courseId, IFormFile thumbnailFile);

        Task<ServiceResponseDTO<GetCourseDTO>> CreateCourse(CreateCourseDTO dto);
        Task<ServiceResponseDTO<List<GetCourseDTO>>> GetPopularCoursesAsync(int count);
    }
}

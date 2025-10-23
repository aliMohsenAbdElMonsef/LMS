using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Lecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ILectureServices: IBaseService<GetLectureDTO, CreateLectureDTO, UpdateLectureDTO>
    {
        Task<IEnumerable<GetLectureDTO>> GetCourseOcturesAsync(string courseId);
        Task<IEnumerable<GetLectureDTO>> GetInstructorOcturesAsync(string instructorId);
    }
}

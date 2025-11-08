using Application.DTOs.User;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IStudentEnrollment : IEnrollIntoCourseServices<ReadStudentEnrollmentDTO,RequestEnrollIntoCourseDTO,UpdateStudentEnrollmentDTO>
    {
        #region Statistics
        Task<ServiceResponseDTO<double>> GetAverageProgressForCourseAsync(string courseId);
        Task<ServiceResponseDTO<List<ReadStudentEnrollmentDTO>>> GetStudentsWithLowProgress(string courseId);

        #endregion  
    }
}

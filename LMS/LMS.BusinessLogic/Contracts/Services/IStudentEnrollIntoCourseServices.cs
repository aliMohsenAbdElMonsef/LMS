using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IStudentEnrollIntoCourseServices : IBaseService<ReadStudentEnrollIntoCourseDTO,
        CreateStudentEnrollIntoCourseDTO, UpdateStudentEnrollIntoCourseDTO>
    {
        #region Enrollment Management
        Task<BasicResponseDTO> EnrollStudentAsync(CreateStudentEnrollIntoCourseDTO dto);
        Task<BasicResponseDTO> UnenrollStudentFromCourseAsync(StudentUnenrollfromCourseDTO dto);
        #endregion

        #region Retrieval Methods
        Task<ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>> GetEnrollmentByIdAsync(GetEnrollmentDTO dto);
        Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentEnrollmentsAsync(string studentId);
        Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetCourseEnrollmentsAsync(string courseId);
        #endregion

        #region Check Methods
        Task<bool> IsStudentEnrolledAsync(GetEnrollmentDTO dto);
        Task<int> GetCourseEnrollmentCountAsync(string courseId);
        #endregion

        #region Statistics
        Task<ServiceResponseDTO<double>> GetAverageProgressForCourseAsync(string courseId);
        Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentsWithLowProgressAsync(GetStudentLessThersholdDTO dto);
        #endregion    }
    }
}




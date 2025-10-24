using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAcess.Contracts;
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
        Task<BasicResponseDTO> UnenrollStudentAsync(string enrollmentId);
        Task<BasicResponseDTO> UnenrollStudentFromCourseAsync(string studentId, string courseId);
        #endregion

        #region Progress Management
        Task<ServiceResponseDTO<double>> UpdateProgressAfterAssignmentAsync(string studentId, string courseId, double assignmentScore, double assignmentWeight);
        Task<ServiceResponseDTO<double>> UpdateProgressAfterQuizAsync(string studentId, string courseId, double quizScore, int correctAnswers, int totalQuestions);
        #endregion

        #region Retrieval Methods
        Task<ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>> GetEnrollmentByIdAsync(string enrollmentId);
        Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentEnrollmentsAsync(string studentId);
        Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetCourseEnrollmentsAsync(string courseId);
        #endregion

        #region Check Methods
        Task<bool> IsStudentEnrolledAsync(string studentId, string courseId);
        Task<int> GetCourseEnrollmentCountAsync(string courseId);
        #endregion

        #region Statistics
        Task<ServiceResponseDTO<double>> GetAverageProgressForCourseAsync(string courseId);
        Task<ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>> GetStudentsWithLowProgressAsync(string courseId, double threshold = 30.0);
        #endregion    }
    }
}




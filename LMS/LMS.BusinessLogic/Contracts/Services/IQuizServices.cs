using LMS.BusinessLogic.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IQuizServices: IBaseService<ReadQuizDTO, CreateQuizDTO, UpdateQuizDTO>
    {
        Task<DTOs.Responses.ServiceResponseDTO<bool>> StartQuizAsync(string quizId, string studentId);
        Task<DTOs.Responses.ServiceResponseDTO<QuizAttemptDTO>> GetQuizForTakingAsync(string quizId, string studentId);
        Task<DTOs.Responses.ServiceResponseDTO<QuizResultDTO>> SubmitQuizAsync(SubmitQuizDTO dto);
        Task<DTOs.Responses.ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetQuizzesByCourseAsync(string courseId);
        Task<DTOs.Responses.ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetQuizzesByInstructorAsync(string instructorId);
        Task<DTOs.Responses.ServiceResponseDTO<StudentQuizStatusDTO>> GetStudentQuizStatusAsync(string quizId, string studentId);
        Task<DTOs.Responses.ServiceResponseDTO<QuizResultDTO>> GetQuizResultAsync(string quizId, string studentId);
        Task<DTOs.Responses.ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>> GetQuizSubmissionsAsync(string quizId);
        Task<DTOs.Responses.ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetStudentQuizzesAsync(string studentId);
        Task<DTOs.Responses.ServiceResponseDTO<bool>> GradeQuizAsync(ManualGradeDTO dto);
    }
}

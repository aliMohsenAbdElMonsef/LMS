using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.Entity.Enums;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IAssignmentServices : IBaseService<ReadAssignmentDTO, CreateAssignmentDTO, UpdateAssignmentDTO>
    {
        Task<ServiceResponseDTO<AssignmentDetailsDTO>> GetAssignmentWithDetailsAsync(string id);
        Task<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>> GetAssignmentsByCourseAsync(string courseId);

        Task<ServiceResponseDTO<StudentAssignmentDTO>> SubmitAssignmentAsync(SubmitAssignmentDTO submission);
        Task<ServiceResponseDTO<StudentAssignmentDTO>> GradeAssignmentAsync(GradeAssignmentDTO grade);
        Task<ServiceResponseDTO<StudentAssignmentDTO>> GetStudentAssignmentAsync(string assignmentId, string studentId);
        Task<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>> GetAssignmentSubmissionsAsync(string assignmentId);
        Task<ServiceResponseDTO<StudentAssignmentDTO>> GetStudentAssignmentByIdAsync(string studentAssignmentId);
        Task<ServiceResponseDTO<bool>> DeleteAssignmentAsync(string id);
        Task<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>> GetSubmissionsByStatusAsync(string assignmentId, AssignmentStatus status);
        Task<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>> GetStudentSubmissionsAsync(string studentId);
        Task<ServiceResponseDTO<IEnumerable<StudentAllAssignmentsDTO>>> GetStudentAllAssignmentsAsync(string studentId);
        Task<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>> GetAssignmentsByInstructorAsync(string instructorId);
    }
}
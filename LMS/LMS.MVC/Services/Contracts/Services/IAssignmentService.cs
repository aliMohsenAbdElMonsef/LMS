using LMS.MVC.Models.ViewModels.Assignment;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IAssignmentService : IBaseMVCServices
    {
        Task<IEnumerable<ReadAssignmentResult>> GetAssignmentsByCourse(string courseId);
        Task<AssignmentDetailsResult> GetAssignmentById(string id);
        Task<StudentAssignmentResult> GetStudentAssignment(string assignmentId, string studentId);
        Task<IEnumerable<StudentAssignmentResult>> GetAssignmentSubmissions(string assignmentId);

        Task<ReadAssignmentResult> GetEditModel(string id);
        ReadAssignmentResult GetCreateModel();
        Task<ReadAssignmentResult> CreateAssignment(ReadAssignmentResult model);
        Task<ReadAssignmentResult> EditAssignment(ReadAssignmentResult model);

        Task<StudentAssignmentResult> SubmitAssignment(StudentAssignmentResult model);
        Task<StudentAssignmentResult> GradeAssignment(StudentAssignmentResult model);
        Task<IEnumerable<StudentAssignmentResult>> GetSubmissionsByStatus(string assignmentId, string status);
        Task<StudentAssignmentResult> GetStudentAssignmentById(string studentAssignmentId);
        Task<bool> DeleteAssignment(string id);
    }
}
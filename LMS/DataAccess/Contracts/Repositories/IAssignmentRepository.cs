using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.Entity.Enums;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface IAssignmentRepository : IBaseRepository<Assignment, string>
    {
        Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(string courseId);
        Task<IEnumerable<StudentAssignment>> GetAssignmentSubmissionsAsync(string assignmentId);
        Task<int> GetAssignmentSubmissionsCountAsync(string assignmentId);
        Task<StudentAssignment?> GetStudentAssignmentAsync(string assignmentId, string studentId);
        Task<StudentAssignment?> GetStudentAssignmentByIdAsync(string studentAssignmentId);
        Task CreateStudentAssignmentAsync(StudentAssignment studentAssignment);
        Task UpdateStudentAssignmentAsync(StudentAssignment studentAssignment);
        Task<IEnumerable<StudentAssignment>> GetSubmissionsByStatusAsync(string assignmentId, AssignmentStatus status);
        Task<int> GetSubmissionsCountByStatusAsync(string assignmentId, AssignmentStatus status);
        Task<IEnumerable<StudentAssignment>> GetStudentSubmissionsAsync(string studentId);
        Task<IEnumerable<Assignment>> GetAssignmentsByInstructorAsync(string instructorId);
    }
}

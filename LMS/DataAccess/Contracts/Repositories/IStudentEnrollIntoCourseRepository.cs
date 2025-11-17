using Domain.Entities.RelationTables;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface IStudentEnrollIntoCourseRepository : IBaseRepository<StudentEnrollIntoCourse, string>
    {
        Task<StudentEnrollIntoCourse> GetFirstOrDefaultAsync(string studentId, string courseId, string? includeProperties = null);
        Task<bool> ExistsAsync(string studentId, string courseId);
        Task<bool> IsStudentEnrolledInCourseAsync(string studentId, string courseId);
    }
}

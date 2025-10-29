using LMS.Entity.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface IInstructorEnrolltoCourseRepository : IBaseRepository<InstructorEnrolltoCourse,string>
    {
        Task<InstructorEnrolltoCourse>? GetByIdWithDetailsAsync(string id);
        Task<List<InstructorEnrolltoCourse>> GetByInstructorIdAsync(string instructorId);
        Task<List<InstructorEnrolltoCourse>> GetPendingEnrollmentsAsync();
        Task<List<InstructorEnrolltoCourse>> GetByCourseIdAsync(string courseId);
        Task<InstructorEnrolltoCourse> GetByInstructorAndCourseAsync(string userId, string courseId);
        Task AddRangeAsync(List<InstructorEnrolltoCourse> instructorEnrollments);
    }
}

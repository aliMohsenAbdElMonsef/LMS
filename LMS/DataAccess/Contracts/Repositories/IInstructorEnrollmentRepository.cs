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
        InstructorEnrolltoCourse? GetByIdWithDetails(string id);
        List<InstructorEnrolltoCourse> GetByInstructorId(string instructorId);
        List<InstructorEnrolltoCourse> GetPendingEnrollments();
        List<InstructorEnrolltoCourse> GetByCourseId(string courseId);
        Task AddRangeAsync(List<InstructorEnrolltoCourse> instructorEnrollments);
    }
}

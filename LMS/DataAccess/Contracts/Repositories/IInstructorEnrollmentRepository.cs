using LMS.Entity.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts.Repositories
{
    public interface IInstructorEnrolltoCourseRepository : IBaseRepository<InstructorEnrolltoCourse, int>
    {
        InstructorEnrolltoCourse? GetByIdWithDetails(string id);
        List<InstructorEnrolltoCourse> GetByInstructorId(string instructorId);
        List<InstructorEnrolltoCourse> GetPendingEnrollments();
        List<InstructorEnrolltoCourse> GetByCourseId(string courseId);
       
    }
}

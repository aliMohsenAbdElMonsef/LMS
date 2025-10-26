using Domain.Entities.RelationTables;
using LMS.Entity.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface IStudentEnrollIntoCourseRepository : IBaseRepository<StudentEnrollIntoCourse, string>
    {
        Task<StudentEnrollIntoCourse> GetFirstOrDefaultAsync(string studentId, string courseId, string? includeProperties = null);
    }
}

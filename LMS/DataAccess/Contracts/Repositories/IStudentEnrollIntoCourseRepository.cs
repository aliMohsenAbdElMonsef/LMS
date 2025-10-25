using Domain.Entities.RelationTables;
using LMS.Entity.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts.Repositories
{
    public interface IStudentEnrollIntoCourseRepository : IBaseRepository<StudentEnrollIntoCourse, string>
    {
    }
}

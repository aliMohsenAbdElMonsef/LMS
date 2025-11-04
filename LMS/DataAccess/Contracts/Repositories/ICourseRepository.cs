using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface ICourseRepository: IBaseRepository<Course,string>
    {
        Course? GetByIdWithAcceptedInstructors(string id);
        Task<List<Course>> GetCoursesByCategoryIdAsync(string id);

        Task<Course?> FindByCodeAsync(string code);
    }

}

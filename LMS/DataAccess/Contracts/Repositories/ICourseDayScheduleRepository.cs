using LMS.Entity.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface ICourseDayScheduleRepository : IBaseRepository<CourseDaySchedule , string>
    {
        
        Task<IEnumerable<CourseDaySchedule>> GetCourseSchedulesAsync(string courseId);
        Task<IEnumerable<CourseDaySchedule>> FindAsync(Expression<Func<CourseDaySchedule, bool>> predicate);
        Task AddRangeAsync(IEnumerable<CourseDaySchedule> entities);
        void RemoveRange(IEnumerable<CourseDaySchedule> entities);
    }
}

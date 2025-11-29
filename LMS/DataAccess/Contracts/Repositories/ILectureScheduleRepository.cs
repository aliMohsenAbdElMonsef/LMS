using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface ILectureScheduleRepository : IBaseRepository<LectureSchedule, string>
    {
        Task<IEnumerable<LectureSchedule>> GetByCourseIdAsync(string courseId);
        Task<LectureSchedule> GetByIdWithLecturesAsync(string id);
        Task<IEnumerable<LectureSchedule>> GetByCourseIdWithLecturesAsync(string courseId);
    }
}

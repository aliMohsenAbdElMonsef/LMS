using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts.Repositories
{
    public interface ILectureRepository : IBaseRepository<Lecture, string>
    {
        Task AddRangeAsync(List<Lecture> lectures);
        Task<IEnumerable<Lecture>> GetCourseOcturesAsync(string courseId);
    }
}

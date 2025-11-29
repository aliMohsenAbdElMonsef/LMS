using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface ILectureRepository : IBaseRepository<Lecture, string>
    {
        Task AddRangeAsync(List<Lecture> lectures);
        Task<IEnumerable<Lecture>> GetCourseLecturesAsync(string courseId);
        Task<IEnumerable<Lecture>> GetLecturesByCourseIdsAsync(IEnumerable<string> courseIds);
    }
}

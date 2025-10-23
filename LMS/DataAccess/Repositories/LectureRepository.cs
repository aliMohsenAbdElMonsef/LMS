using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAcess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class LectureRepository: BaseRepository<Lecture,string>, ILectureRepository
    {
        public LectureRepository(LMSDbContext db) : base(db)
        {
        }
        public async Task<IEnumerable<Lecture>> GetCourseOcturesAsync(string courseId)
        {
            return await _set
                 .AsNoTracking()
                 .Where(a => a.CourseId == courseId && !a.IsDeleted)
                 .OrderBy(a => a.StartTime)
                 .ToListAsync();
        }
    }
}

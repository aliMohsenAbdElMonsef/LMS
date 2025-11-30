using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAccess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Repositories
{
    internal class LectureRepository: BaseRepository<Lecture,string>, ILectureRepository
    {
        public LectureRepository(LMSDbContext db) : base(db)
        {
        }
        public async Task<IEnumerable<Lecture>> GetCourseLecturesAsync(string courseId)
        {
            return await _set
                 .Where(a => a.CourseId == courseId && !a.IsDeleted)
                 .OrderBy(a => a.LectureDate)
                 .ThenBy(a => a.StartTime)
                 .ToListAsync();
        }

        public async Task<IEnumerable<Lecture>> GetLecturesByCourseIdsAsync(IEnumerable<string> courseIds)
        {
            return await _set
                 .Include(l => l.Course) // Include Course to get Course Name
                 .Where(l => courseIds.Contains(l.CourseId) && !l.IsDeleted)
                 .OrderBy(l => l.LectureDate)
                 .ThenBy(l => l.StartTime)
                 .ToListAsync();
        }

        public async Task AddRangeAsync(List<Lecture> lectures)
        {
            await _set.AddRangeAsync(lectures);
        }
    }
}

using DataAccess.Context;
using LMS.DataAccess.Contracts.Repositories;
using Domain.Entities.MainEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Repositories
{
    internal class LectureScheduleRepository : BaseRepository<LectureSchedule, string>, ILectureScheduleRepository
    {
        private readonly LMSDbContext _context;

        public LectureScheduleRepository(LMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LectureSchedule>> GetByCourseIdAsync(string courseId)
        {
            return await _context.LectureSchedules
                .Where(ls => ls.CourseId == courseId && !ls.IsDeleted)
                .OrderBy(ls => ls.DayOfWeek)
                .ThenBy(ls => ls.StartTime)
                .ToListAsync();
        }

        public async Task<LectureSchedule> GetByIdWithLecturesAsync(string id)
        {
            return await _context.LectureSchedules
                .Include(ls => ls.Lectures)
                .FirstOrDefaultAsync(ls => ls.Id == id && !ls.IsDeleted);
        }

        public async Task<IEnumerable<LectureSchedule>> GetByCourseIdWithLecturesAsync(string courseId)
        {
            return await _context.LectureSchedules
                .Include(ls => ls.Lectures)
                .Include(ls => ls.Instructor)
                .Where(ls => ls.CourseId == courseId && !ls.IsDeleted)
                .OrderBy(ls => ls.DayOfWeek)
                .ThenBy(ls => ls.StartTime)
                .ToListAsync();
        }
    }
}

using DataAccess.Context;
using LMS.DataAcess.Contracts.Repositories;
using LMS.Entity.Entities.MainEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class CourseDayScheduleRepository :BaseRepository<CourseDaySchedule ,string> ,ICourseDayScheduleRepository
    {
        protected readonly LMSDbContext _db;
        protected readonly DbSet<CourseDaySchedule> _set;

        public CourseDayScheduleRepository(LMSDbContext db) : base(db) 
        {
            _db = db;
            _set=db.Set<CourseDaySchedule>();
        }

        public async Task<CourseDaySchedule?> GetByIdAsync(string id)
        {
            if (id == null) return null;
            else
            {
                return await _set
                   .Include(cds => cds.Instructor)
                   .FirstOrDefaultAsync(cds => cds.ID == id && !cds.IsDeleted);
            }
        }

        public async Task<IEnumerable<CourseDaySchedule>> GetCourseSchedulesAsync(string courseId)
        {
            return await _set
                .AsNoTracking()
                .Include(cds => cds.Instructor)
                .Where(cds => cds.CourseId == courseId && !cds.IsDeleted)
                .OrderBy(cds => cds.DayOfWeek)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseDaySchedule>> FindAsync(Expression<Func<CourseDaySchedule, bool>> predicate)
        {
            return await _set
                .AsNoTracking()
                .Include(cds => cds.Instructor)
                .Where(predicate)
                .Where(cds => !cds.IsDeleted)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<CourseDaySchedule> entities)
        {
            await _set.AddRangeAsync(entities);
        }

       
        public void RemoveRange(IEnumerable<CourseDaySchedule> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
            }
            _db.Set<CourseDaySchedule>().UpdateRange(entities);
        }
    }
}

using DataAccess.Context;
using Domain.Entities.RelationTables;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Repositories
{
    internal class StudentEnrollIntoCourseRepository : BaseRepository<StudentEnrollIntoCourse, string>, IStudentEnrollIntoCourseRepository
    {
        public StudentEnrollIntoCourseRepository(LMSDbContext context) : base(context)
        {


        }
       public async Task<StudentEnrollIntoCourse?> GetFirstOrDefaultAsync(string studentId, string courseId, string? includeProperties = null)
        {
            IQueryable<StudentEnrollIntoCourse> query = _set
                .Where(e => e.StudentId == studentId && e.CourseId == courseId);

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }

    }
}

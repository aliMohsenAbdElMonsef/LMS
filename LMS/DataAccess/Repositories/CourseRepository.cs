using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAcess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using LMS.Entity.Entities.RelationTables;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class CourseRepository: BaseRepository<Course, string>, ICourseRepository
    {
        public CourseRepository(LMSDbContext db) : base(db)
        {
        }
        public Course? GetByIdWithAcceptedInstructors(string id)
        {
            return _set
                .Include(c => c.InstructorEnrollments
                    .Where(e => e.Status == ApplicationStatus.Approved))
                    .ThenInclude(e => e.Instructor)
                .FirstOrDefault(c => c.Id == id);
        }

    }
}

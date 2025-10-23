using DataAccess.Context;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.DataAcess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class InstructorEnrolltoCourseRepository : BaseRepository<InstructorEnrolltoCourse, string>,
        IInstructorEnrolltoCourseRepository
    {
        public InstructorEnrolltoCourseRepository(LMSDbContext context) : base(context)
        {
        }

        public InstructorEnrolltoCourse? GetByIdWithDetails(string id)
        {
            return _set
                .Include(e => e.Instructor)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.Id == id);
        }

        public List<InstructorEnrolltoCourse> GetByInstructorId(string instructorId)
        {
            return _set
                .Include(e => e.Course)
                .Where(e => e.InstructorId == instructorId)
                .OrderByDescending(e => e.RequestedAt)
                .ToList();
        }

        public List<InstructorEnrolltoCourse> GetPendingEnrollments()
        {
            return _set
                .Include(e => e.Instructor)
                .Include(e => e.Course)
                .Where(e => e.Status == ApplicationStatus.Pending)
                .OrderBy(e => e.RequestedAt)
                .ToList();
        }

        public List<InstructorEnrolltoCourse> GetByCourseId(string courseId)
        {
            return _set
                .Include(e => e.Instructor)
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.RequestedAt)
                .ToList();
        }

        public async Task AddRangeAsync(List<InstructorEnrolltoCourse> instructorEnrollments)
        {
            await _set.AddRangeAsync(instructorEnrollments);
        }
    }
}

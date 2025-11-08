using DataAccess.Context;
using Domain.Enums;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;
using Microsoft.EntityFrameworkCore;

namespace LMS.DataAccess.Repositories
{
    internal class InstructorEnrolltoCourseRepository : BaseRepository<InstructorEnrolltoCourse, string>,
        IInstructorEnrolltoCourseRepository
    {
        public InstructorEnrolltoCourseRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<InstructorEnrolltoCourse?> GetByIdWithDetailsAsync(string id)
        {
            return await _set
                .Include(e => e.Instructor)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<InstructorEnrolltoCourse>> GetByInstructorIdAsync(string instructorId)
        {
            return await _set
                .Include(e => e.Course)
                .Where(e => e.InstructorId == instructorId)
                .OrderByDescending(e => e.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<InstructorEnrolltoCourse>> GetPendingEnrollmentsAsync()
        {
            return await _set
                .Include(e => e.Instructor)
                .Include(e => e.Course)
                .Where(e => e.Status == ApplicationStatus.Pending)
                .OrderBy(e => e.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<InstructorEnrolltoCourse>> GetByCourseIdAsync(string courseId)
        {
            return await _set
                .Include(e => e.Instructor)
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.RequestedAt)
                .ToListAsync();
        }

        public async Task AddRangeAsync(List<InstructorEnrolltoCourse> instructorEnrollments)
        {
            await _set.AddRangeAsync(instructorEnrollments);
        }

        public async Task<bool> ExistsAsync(string instructorId, string courseId)
        {
            return await _set
                .AnyAsync(e => e.InstructorId == instructorId && e.CourseId == courseId);
        }

        public async Task<InstructorEnrolltoCourse?> GetByInstructorAndCourseAsync(string instructorId, string courseId)
        {
            return await _set
                .Include(e => e.Instructor)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.InstructorId == instructorId && e.CourseId == courseId);
        }

        public async Task<List<InstructorEnrolltoCourse>> GetApprovedByInstructorIdAsync(string instructorId)
        {
            return await _set
                .Include(e => e.Course)
                .Where(e => e.InstructorId == instructorId && e.Status == ApplicationStatus.Approved)
                .OrderByDescending(e => e.RequestedAt)
                .ToListAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _set
                .CountAsync(e => e.Status == ApplicationStatus.Pending);
        }

       
    }
}
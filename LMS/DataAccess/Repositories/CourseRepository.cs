using DataAccess.Context;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.DataAccess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMS.DataAccess.Repositories
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

        public override async Task<Course?> FindByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return await _set
                .Include(c => c.Admin)
                .Include(c => c.Category)
                .Include(c => c.Students)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Instructor)
                .Include(c => c.InstructorEnrollments)
                    .ThenInclude(ie => ie.Instructor)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Course>> GetCoursesByCategoryIdAsync(string id)
        {
            return await _set
                .Where(c => c.CategoryId == id)
                .Include(c => c.InstructorEnrollments
                .Where(e => e.Status == ApplicationStatus.Approved))
                .ThenInclude(e => e.Instructor)
                .Include(c => c.Category)
                .ToListAsync();
        }

        public Task<Course?> FindByCodeAsync(string code)
        {
            return _set.FirstOrDefaultAsync(c => c.CourseCode == code);
        }
        public override async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _set.Include(c=>c.Admin).Include(c=>c.Category).ToListAsync();
        }
    }
}

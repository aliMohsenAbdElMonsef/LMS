using DataAccess.Context;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.DataAccess.Repositories
{
    internal class AssignmentRepository : BaseRepository<Assignment, string>, IAssignmentRepository
    {
        public AssignmentRepository(LMSDbContext context) : base(context)
        {
        }

        public override async Task<Assignment?> FindByIdAsync(string id)
        {
            return await _set
                .Include(a => a.Course)
                .Include(a => a.Instructor)
                .Include(a => a.Students)
                    .ThenInclude(sa => sa.Student)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted); 
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(string courseId)
        {
            return await _set
                .Include(a => a.Course)
                .Include(a => a.Instructor)
                .Where(a => a.CourseId == courseId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentAssignment>> GetAssignmentSubmissionsAsync(string assignmentId)
        {
            return await _db.StudentAssignments
                .Include(sa => sa.Student)
                .Include(sa => sa.Assignment)
                .Where(sa => sa.AssignmentId == assignmentId && !sa.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> GetAssignmentSubmissionsCountAsync(string assignmentId)
        {
            return await _db.StudentAssignments
                .CountAsync(sa => sa.AssignmentId == assignmentId && !sa.IsDeleted);
        }

        public async Task<StudentAssignment?> GetStudentAssignmentAsync(string assignmentId, string studentId)
        {
            return await _db.StudentAssignments
                .Include(sa => sa.Student)
                .Include(sa => sa.Assignment)
                .FirstOrDefaultAsync(sa => sa.AssignmentId == assignmentId &&
                                         sa.StudentId == studentId &&
                                         !sa.IsDeleted);
        }

        public async Task<StudentAssignment?> GetStudentAssignmentByIdAsync(string studentAssignmentId)
        {
            return await _db.StudentAssignments
                .Include(sa => sa.Student)
                .Include(sa => sa.Assignment)
                .FirstOrDefaultAsync(sa => sa.Id == studentAssignmentId && !sa.IsDeleted);
        }

        public async Task CreateStudentAssignmentAsync(StudentAssignment studentAssignment)
        {
            await _db.StudentAssignments.AddAsync(studentAssignment);
        }

        public async Task UpdateStudentAssignmentAsync(StudentAssignment studentAssignment)
        {
            _db.StudentAssignments.Update(studentAssignment);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<StudentAssignment>> GetSubmissionsByStatusAsync(string assignmentId, AssignmentStatus status)
        {
            return await _db.StudentAssignments
                .Include(sa => sa.Student)
                .Include(sa => sa.Assignment)
                .Where(sa => sa.AssignmentId == assignmentId &&
                            sa.Status == status &&
                            !sa.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> GetSubmissionsCountByStatusAsync(string assignmentId, AssignmentStatus status)
        {
            return await _db.StudentAssignments
                .CountAsync(sa => sa.AssignmentId == assignmentId &&
                                 sa.Status == status &&
                                 !sa.IsDeleted);
        }
    }
}
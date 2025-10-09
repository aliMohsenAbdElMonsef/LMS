using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Context
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser,ApplicationRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        // main entities
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Skills> Skills { get; set; }
        public DbSet<CertificateTemplate> CertificateTemplates { get; set; }

        // relation entities 
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<StudentCertificate> StudentCertificates { get; set; }
        public DbSet<InstructorCourse> InstructorCourses { get; set; }
        public DbSet<StudentEnrollIntoCourse> StudentEnrollments { get; set; }
        public DbSet<StudentLecture> StudentLectures { get; set; }
        public DbSet<StudentQuiz> StudentQuizzes { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<StudentAnswerQuestion> StudentAnswers { get; set; }
        public DbSet<CourseSkill>CourseSkills { get; set; }

    }
}

using Domain.Entities;
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
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
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
        public DbSet<CourseSkill> CourseSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //global gilters for soft-deleted entities
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Lecture>().HasQueryFilter(l => !l.IsDeleted);
            builder.Entity<Quiz>().HasQueryFilter(q => !q.IsDeleted);
            builder.Entity<Question>().HasQueryFilter(q => !q.IsDeleted);
            builder.Entity<Assignment>().HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Skills>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<CertificateTemplate>().HasQueryFilter(ct => !ct.IsDeleted);

            //student enrolls into course
            builder.Entity<StudentEnrollIntoCourse>()
                .HasKey(se => new { se.StudentId, se.CourseId });

            builder.Entity<StudentEnrollIntoCourse>()
                .HasOne(se => se.Student)
                .WithMany(s => s.Enrollment)
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentEnrollIntoCourse>()
                .HasOne(se => se.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(se => se.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            //course reviews
            builder.Entity<CourseReview>()
                .HasKey(cr => cr.Id);

            builder.Entity<CourseReview>()
                .HasOne(cr => cr.Student)
                .WithMany(s => s.Reviews)
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CourseReview>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            //course skill 
            builder.Entity<CourseSkill>()
                .HasKey(cs => new { cs.CourseId, cs.SkillId });

            builder.Entity<CourseSkill>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.Skills)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseSkill>()
                .HasOne(cs => cs.Skill)
                .WithMany(s => s.Courses)
                .HasForeignKey(cs => cs.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            //instructor course
            builder.Entity<InstructorCourse>()
                .HasKey(ic => new { ic.InstructorId, ic.CourseId });

            builder.Entity<InstructorCourse>()
                .HasOne(ic => ic.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(ic => ic.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InstructorCourse>()
                .HasOne(ic => ic.Course)
                .WithMany(c => c.Instructors)
                .HasForeignKey(ic => ic.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            //student answer question
            builder.Entity<StudentAnswerQuestion>()
                .HasKey(saq => new { saq.StudentId, saq.QuestionId });

            builder.Entity<StudentAnswerQuestion>()
                .HasOne(saq => saq.Student)
                .WithMany(s => s.Answers)
                .HasForeignKey(saq => saq.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentAnswerQuestion>()
                .HasOne(saq => saq.Question)
                .WithMany(q => q.StudentAnswers)
                .HasForeignKey(saq => saq.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            //student assignment
            builder.Entity<StudentAssignment>()
                .HasKey(sa => new { sa.StudentId, sa.AssignmentId });

            builder.Entity<StudentAssignment>()
                .HasOne(sa => sa.Student)
                .WithMany(s => s.UploadedAssignemnts)
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentAssignment>()
                .HasOne(sa => sa.Assignment)
                .WithMany(a => a.Students)
                .HasForeignKey(sa => sa.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            //student certificate template
            builder.Entity<StudentCertificate>()
                .HasKey(sc => new { sc.StudentId, sc.certificateTamplateId });

            builder.Entity<StudentCertificate>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.EarnedCertificates)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentCertificate>()
                .HasOne(sc => sc.CertificateTemplate)
                .WithMany(ct => ct.studentCertificates)
                .HasForeignKey(sc => sc.certificateTamplateId)
                .OnDelete(DeleteBehavior.Restrict);

            //student lecture
            builder.Entity<StudentLecture>()
                .HasKey(sl => new { sl.StudentId, sl.LectureId });

            builder.Entity<StudentLecture>()
                .HasOne(sl => sl.Student)
                .WithMany(s => s.LecturesAttended)
                .HasForeignKey(sl => sl.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentLecture>()
                .HasOne(sl => sl.Lecture)
                .WithMany(l => l.Students)
                .HasForeignKey(sl => sl.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // student quiz
            builder.Entity<StudentQuiz>()
                .HasKey(sq => new { sq.StudentId, sq.QuizId });

            builder.Entity<StudentQuiz>()
                .HasOne(sq => sq.Student)
                .WithMany(s => s.QuizzesAttended)
                .HasForeignKey(sq => sq.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentQuiz>()
                .HasOne(sq => sq.Quiz)
                .WithMany(q => q.Students)
                .HasForeignKey(sq => sq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            HandleSoftDelete();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return await base.SaveChangesAsync(cancellationToken);
        }
        private void HandleSoftDelete()
        {
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
            }

        }
    }
}

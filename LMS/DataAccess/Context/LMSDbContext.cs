using Domain.Entities;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.Entity.Entities.MainEntities;
using LMS.Entity.Entities.RelationTables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection.Emit;

namespace DataAccess.Context
{
    internal class LMSDbContextFactory : IDesignTimeDbContextFactory<LMSDbContext>
    {
        public LMSDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LMSDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\ALIMOHSEN;Database=LMS;Trusted_Connection=true;TrustServerCertificate=true");

            return new LMSDbContext(optionsBuilder.Options);
        }
    }

    internal class LMSDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {

        public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options)
        {
            // Add this constructor to see what's being passed
            if (Database.IsSqlServer())
            {
                try
                {
                    var connection = Database.GetDbConnection();
                    Console.WriteLine($"DbContext Connection String: {connection.ConnectionString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting connection string: {ex.Message}");
                }
            }
        }


        // Main Entities
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

        public DbSet<BlackListedTokens> BlackListedTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Skills> Skills { get; set; }
        public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
        public DbSet<LectureSchedule> LectureSchedules { get; set; }

        // Relation Entities
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<CourseDaySchedule> CourseDaySchedules { get; set; }
        public DbSet<StudentCertificate> StudentCertificates { get; set; }
        public DbSet<InstructorEnrolltoCourse> InstructorEnrollments { get; set; }
        public DbSet<StudentEnrollIntoCourse> StudentEnrollments { get; set; }
        public DbSet<StudentLecture> StudentLectures { get; set; }
        public DbSet<StudentQuiz> StudentQuizzes { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<StudentAnswerQuestion> StudentAnswers { get; set; }
        public DbSet<CourseSkill> CourseSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // global query filters - ADDED ALL RELATIONSHIP ENTITIES
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Lecture>().HasQueryFilter(l => !l.IsDeleted);
            builder.Entity<Quiz>().HasQueryFilter(q => !q.IsDeleted);
            builder.Entity<Question>().HasQueryFilter(q => !q.IsDeleted);
            builder.Entity<Assignment>().HasQueryFilter(a => !a.IsDeleted);       
            builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Skills>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<CourseSkill>().HasQueryFilter(cs => !cs.IsDeleted);

            // ADDED: Decimal precision for Price
            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            // with admin
            builder.Entity<Course>()
                .HasOne(c => c.Admin)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>()
                .HasOne(cat => cat.Admin)
                .WithMany(u => u.Categories)
                .HasForeignKey(cat => cat.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Skills>()
                .HasOne(s => s.Admin)
                .WithMany(u => u.CreatedSkills)
                .HasForeignKey(s => s.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CertificateTemplate>()
                .HasOne(ct => ct.Admin)
                .WithMany(u => u.certificateTemplates)
                .HasForeignKey(ct => ct.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
            // end 

            // with course
            builder.Entity<Category>()
                .HasMany(cat => cat.Courses)
                .WithOne(c => c.Category)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasOne(c => c.CertificateTemplate)
                .WithOne(ct => ct.Course)
                .HasForeignKey<CertificateTemplate>(ct => ct.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasMany(c => c.Lectures)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasMany(c => c.Assignments)
                .WithOne(a => a.Course)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasMany(c => c.Quizzes)
                .WithOne(q => q.Course)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            // end

            // with assignment
            builder.Entity<Assignment>()
                .HasOne(a => a.Instructor)
                .WithMany(u => u.CreatedAssignments)
                .HasForeignKey(a => a.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
            //end

            // with lecture
            builder.Entity<Lecture>()
                .HasOne(l => l.AssignedInstructor)
                .WithMany(u => u.TeachedLectures)
                .HasForeignKey(l => l.AssignedInstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lecture>()
                .HasOne(l => l.LastUploadedByInstructor)
                .WithMany(u => u.LecturesLastUploaded)
                .HasForeignKey(l => l.LastUploadedByInstructorId)
                .OnDelete(DeleteBehavior.Restrict);
            // end

            // with quiz
            builder.Entity<Quiz>()
                .HasOne(q => q.Instructor)
                .WithMany(u => u.CreatedQuizzes)
                .HasForeignKey(q => q.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(z => z.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Restrict);
            // end

            // with lecture schedule
            builder.Entity<LectureSchedule>()
                .HasOne(ls => ls.Course)
                .WithMany(c => c.LectureSchedules)
                .HasForeignKey(ls => ls.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Lecture>()
                .HasOne(l => l.LectureSchedule)
                .WithMany(ls => ls.Lectures)
                .HasForeignKey(l => l.LectureScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
            // end

            // student enroll into course
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

            // course review
            builder.Entity<CourseReview>()
                .HasKey(cr => cr.Id);

            builder.Entity<CourseReview>()
                .HasOne(cr => cr.Student)
                .WithMany(s => s.Reviews)
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseReview>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // course skill
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

            // InstructorEnrollment configuration
            builder.Entity<InstructorEnrolltoCourse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Instructor)
                    .WithMany(u => u.Courses)
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.InstructorEnrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Prevent duplicate pending enrollments
                entity.HasIndex(e => new { e.InstructorId, e.CourseId, e.Status });
            });

            // student answer question
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

            // student assignment
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

            // student certificate
            builder.Entity<StudentCertificate>()
                .HasKey(sc => new { sc.StudentId, sc.certificateTamplateId });

            builder.Entity<StudentCertificate>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.EarnedCertificates)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentCertificate>()
                .HasOne(sc => sc.CertificateTemplate)
                .WithMany(ct => ct.StudentCertificates)
                .HasForeignKey(sc => sc.certificateTamplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // student lecture
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
                .HasKey(sq => sq.Id);

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
            HandleSoftDeleteForCategories();
            HandleSoftDeleteForUser();
            HandleSoftDeleteForCourses();
            HandleSoftDeleteForSkills();
            HandleSoftDeleteForLectureSchedules();
            HandleSoftDeleteForQuestions();
            HandleSoftDeleteForQuizzes();
            HandleSoftDeleteForLectures();
            HandleSoftDeleteForAssignments();
            HandleSoftDeleteForCertificateTemplates();
            // ADDED: Handle soft delete for relationship entities
            HandleSoftDeleteForRelationshipEntities();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDeleteForCategories();
            HandleSoftDeleteForUser();
            HandleSoftDeleteForCourses();
            HandleSoftDeleteForSkills();
            HandleSoftDeleteForLectureSchedules();
            HandleSoftDeleteForQuestions();
            HandleSoftDeleteForQuizzes();
            HandleSoftDeleteForLectures();
            HandleSoftDeleteForAssignments();
            HandleSoftDeleteForCertificateTemplates();
            // ADDED: Handle soft delete for relationship entities
            HandleSoftDeleteForRelationshipEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        // ADDED: Combined method to handle soft delete for all relationship entities
        private void HandleSoftDeleteForRelationshipEntities()
        {
            HandleSoftDeleteForEntity<CourseReview>();
            HandleSoftDeleteForEntity<StudentCertificate>();
            HandleSoftDeleteForEntity<InstructorEnrolltoCourse>();
            HandleSoftDeleteForEntity<StudentEnrollIntoCourse>();
            HandleSoftDeleteForEntity<StudentLecture>();
            HandleSoftDeleteForEntity<StudentQuiz>();
            HandleSoftDeleteForEntity<StudentAssignment>();
            HandleSoftDeleteForEntity<StudentAnswerQuestion>();
            HandleSoftDeleteForEntity<CourseSkill>();
        }

        // ADDED: Generic method to handle soft delete for any entity
        private void HandleSoftDeleteForEntity<T>() where T : class
        {
            var entities = ChangeTracker.Entries<T>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);

            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                var softDeletionEntity = entry.Entity as SoftDeletion;
                if (softDeletionEntity != null)
                {
                    softDeletionEntity.IsDeleted = true;
                    softDeletionEntity.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        private void HandleSoftDeleteForCategories()
        {
            var entities = ChangeTracker.Entries<Category>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);

            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
            }
        }
        private void HandleSoftDeleteForSkills()
        {
            var entities = ChangeTracker.Entries<Skills>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
            }
        }
        private void HandleSoftDeleteForCourses()
        {
            var entities = ChangeTracker.Entries<Course>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);

            foreach (var entry in entities)
            {
                var course = entry.Entity;

                if (course.CertificateTemplate != null)
                {
                    course.CertificateTemplate.IsDeleted = true;
                    course.CertificateTemplate.DeletedAt = DateTime.UtcNow;
                }

                foreach (var lectureSchedule in course.LectureSchedules)
                {
                    lectureSchedule.IsDeleted = true;
                    lectureSchedule.DeletedAt = DateTime.UtcNow;
                    foreach (var lecture in lectureSchedule.Lectures)
                    {
                        lecture.IsDeleted = true;
                        lecture.DeletedAt = DateTime.UtcNow;
                    }
                }

                foreach (var assignment in course.Assignments)
                {
                    assignment.IsDeleted = true;
                    assignment.DeletedAt = DateTime.UtcNow;
                }

                foreach (var quiz in course.Quizzes)
                {
                    quiz.IsDeleted = true;
                    quiz.DeletedAt = DateTime.UtcNow;
                    foreach (var question in quiz.Questions)
                    {
                        question.IsDeleted = true;
                        question.DeletedAt = DateTime.UtcNow;
                    }
                }

                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
            }
        }

        private void HandleSoftDeleteForUser()
        {
            var entities = ChangeTracker.Entries<ApplicationUser>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (var entry in entities)
            {
                var user = entry.Entity;

                entry.State = EntityState.Modified;
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
            }
        }

        private void HandleSoftDeleteForCertificateTemplates()
        {
            var entities = ChangeTracker.Entries<CertificateTemplate>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
            }
        }

        private void HandleSoftDeleteForAssignments()
        {
            var entities = ChangeTracker.Entries<Assignment>()
        .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);

            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;

                // Also soft delete related student assignments
                var assignment = entry.Entity;
                foreach (var studentAssignment in assignment.Students)
                {
                    studentAssignment.IsDeleted = true;
                    studentAssignment.DeletedAt = DateTime.UtcNow;
                }
            }
        }
        private void HandleSoftDeleteForLectures()
        {
            var entities = ChangeTracker.Entries<Lecture>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
            }
        }
        private void HandleSoftDeleteForQuizzes()
        {
            var entities = ChangeTracker.Entries<Quiz>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
                foreach (var question in entry.Entity.Questions)
                {
                    question.IsDeleted = true;
                    question.DeletedAt = DateTime.UtcNow;
                }
            }
        }
        private void HandleSoftDeleteForQuestions()
        {
            var entities = ChangeTracker.Entries<Question>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
            }
        }
        private void HandleSoftDeleteForLectureSchedules()
        {
            var entities = ChangeTracker.Entries<LectureSchedule>()
                .Where(e => e.State == EntityState.Deleted && e.Entity is SoftDeletion);
            foreach (var entry in entities)
            {
                entry.State = EntityState.Modified;
                ((SoftDeletion)entry.Entity).IsDeleted = true;
                ((SoftDeletion)entry.Entity).DeletedAt = DateTime.UtcNow;
                foreach (var lecture in entry.Entity.Lectures)
                {
                    lecture.IsDeleted = true;
                    lecture.DeletedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
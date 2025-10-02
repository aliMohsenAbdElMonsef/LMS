using LMS.Models.DataModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected ApplicationDbContext() { }

        // === DbSets for all entities ===
        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<CourseSkill> CourseSkills { get; set; }
        public DbSet<CourseInstructor> CourseInstructors { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===== Course =====
            builder.Entity<Course>()
                .HasKey(c => c.Id);

            builder.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();

            builder.Entity<Course>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Entity<Course>()
                .Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Entity<Course>()
                .Property(c => c.Language)
                .IsRequired()
                .HasMaxLength(50);

            builder.Entity<Course>()
                .Property(c => c.StartDate)
                .IsRequired();

            builder.Entity<Course>()
                .Property(c => c.EndDate)
                .IsRequired();

            builder.Entity<Course>()
                .Property(c => c.DurationWeeks)
                .IsRequired();

            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(8,2)");

            // ===== Assignment =====
            builder.Entity<Assignment>()
                .HasKey(a => a.Id);

            builder.Entity<Assignment>()
                .Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Entity<Assignment>()
                .Property(a => a.DueDate)
                .IsRequired();

            builder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Category =====
            builder.Entity<Category>()
                .HasKey(cat => cat.Id);

            builder.Entity<Category>()
                .HasIndex(cat => cat.Name)
                .IsUnique();

            builder.Entity<Category>()
                .Property(cat => cat.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Category>()
                .Property(cat => cat.Description)
                .HasMaxLength(500);

            // ===== Skill =====
            builder.Entity<Skill>()
                .HasKey(s => s.Id);

            builder.Entity<Skill>()
                .HasIndex(s => s.Name)
                .IsUnique();

            builder.Entity<Skill>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Skill>()
                .Property(s => s.Description)
                .HasMaxLength(500);

            // ===== CourseInstructor (many-to-many) =====
            builder.Entity<CourseInstructor>()
                .HasKey(ci => new { ci.InstructorId, ci.CourseId });

            builder.Entity<CourseInstructor>()
                .HasOne(ci => ci.Course)
                .WithMany(c => c.CourseInstructors)
                .HasForeignKey(ci => ci.CourseId);

            builder.Entity<CourseInstructor>()
                .HasOne(ci => ci.Instructor)
                .WithMany(u => u.CourseInstructors)
                .HasForeignKey(ci => ci.InstructorId);

            // ===== CourseSkill (many-to-many) =====
            builder.Entity<CourseSkill>()
                .HasKey(cs => new { cs.CourseId, cs.SkillId });

            builder.Entity<CourseSkill>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.Skills)
                .HasForeignKey(cs => cs.CourseId);

            builder.Entity<CourseSkill>()
                .HasOne(cs => cs.Skill)
                .WithMany(s => s.Courses)
                .HasForeignKey(cs => cs.SkillId);
            // ===== CoursePrerequisite (self-referencing many-to-many) =====
            builder.Entity<CoursePrerequisite>()
            .HasKey(cp => new { cp.CourseId, cp.PrerequisiteCourseId });

            builder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.PrerequisiteCourse)
                .WithMany(c => c.IsPrerequisiteFor)
                .HasForeignKey(cp => cp.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}


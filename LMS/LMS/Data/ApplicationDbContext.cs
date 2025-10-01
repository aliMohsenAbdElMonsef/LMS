using LMS.Models.DataModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser,ApplicationRole,string>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }
        DbSet<Course> Courses { get; set; }
        DbSet<CourseInstructor> CourseInstructors { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity <CourseInstructor>().HasKey(ci => new {ci.InstructorId, ci.CourseId});
            builder.Entity<CourseInstructor>().HasOne(ci => ci.Course)
                .WithMany( ci => ci.courseInstructors)
                .HasForeignKey(ci => ci.CourseId);
            
        }

        }
}

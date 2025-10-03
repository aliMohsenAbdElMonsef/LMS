using LMS.Models.DataModels;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public ApplicationDbContext()
        {
        }        
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<course> courses { get; set; }
        public DbSet<skill> Skills { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {  
            optionsBuilder
                    .UseSqlServer("Data Source=.;Initial Catalog=LMS;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // One-to-one: User → UserRole
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithOne(ur => ur.User)
                .HasForeignKey<UserRole>(ur => ur.UserId);

            // One-to-many: Role → UserRoles
            modelBuilder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);

            // Configure decimal precision for course entity
            modelBuilder.Entity<course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2); // 18 total digits, 2 decimal places

            modelBuilder.Entity<course>()
                .Property(c => c.AverageRating)
                .HasPrecision(3, 2); // 3 total digits, 2 decimal places (0.00 to 9.99)

        }
    }
}

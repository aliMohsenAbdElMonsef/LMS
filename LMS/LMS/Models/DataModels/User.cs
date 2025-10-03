using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models.DataModels
{
    [Table("Users")] 
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(256)]
        public required string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        public UserRole UserRole { get; set; }

        // Navigation: one instructor → many courses
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
    
}

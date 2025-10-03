using System.ComponentModel.DataAnnotations;

namespace LMS.Models.DataModels
{
    public class Role
    {

        [Key]
        public string RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }

        // Navigation: 1 role → many userRoles
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

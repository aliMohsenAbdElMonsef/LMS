using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models.DataModels
{
    public class UserRole
    {
        [ForeignKey("User")]
        public int UserId { get; set; }   // also PK & FK → User

        [ForeignKey("Role")]
        public string RoleId { get; set; }   // FK → Role

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
    }
}

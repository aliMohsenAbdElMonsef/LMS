using Microsoft.AspNetCore.Identity;

namespace LMS.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? UserImage {  get; set; }
        public string ApplyAs { get; set; }
        public string Status { get; set; } = "Pending";
        public List<CourseInstructor> CourseInstructors { get; set; } = new();
    }
}

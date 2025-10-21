using Domain.Enums;

namespace LMS.MVC.Models.ViewModels.User
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserImage { get; set; }
        public UserType ApplyAs { get; set; }
        public ApplicationStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

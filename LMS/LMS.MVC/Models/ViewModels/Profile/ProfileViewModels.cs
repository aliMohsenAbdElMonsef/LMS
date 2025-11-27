using Domain.Enums;

namespace LMS.MVC.Models.ViewModels.Profile
{
    public class ProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? UserImage { get; set; }
        public UserType ApplyAs { get; set; }
        public ApplicationStatus Status { get; set; }
    }

    public class UpdateProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public IFormFile? ProfilePicture { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ProfileIndexViewModel
    {
        public ProfileViewModel Profile { get; set; } = new();
        public UserStatsViewModel Stats { get; set; } = new();
    }

    public class UserStatsViewModel
    {
        public int EnrolledCoursesCount { get; set; }
        public int CompletedCoursesCount { get; set; }
        public int CertificatesCount { get; set; }
        public int AssignmentsSubmitted { get; set; }
        public int QuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        
        // Instructor specific
        public int CreatedCoursesCount { get; set; }
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
    }
}

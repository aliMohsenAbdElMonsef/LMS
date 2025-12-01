using Domain.Enums;

namespace LMS.MVC.Models.ViewModels.Enrollment
{
    public class ReadEnrollmentViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public string CourseCode { get; set; } = string.Empty;

        public string Role { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }
    }
}

namespace LMS.MVC.Models.ViewModels.Enrollment
{
    public class EnrollmentManagementRequest
    {
        public string? UserSearch { get; set; }
        public string? CourseSearch { get; set; }
        public string? SelectedStatus { get; set; }
        public string? Role { get; set; }

        public List<MVCStatusOptions> StatusList { get; set; } = new List<MVCStatusOptions>();
        
    }
}

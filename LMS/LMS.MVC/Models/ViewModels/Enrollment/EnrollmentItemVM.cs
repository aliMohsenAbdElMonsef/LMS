namespace LMS.MVC.Models.ViewModels.Enrollment
{
    public class EnrollmentItemVM
    {
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public string CourseId {  get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

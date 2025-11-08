namespace LMS.MVC.Models.ViewModels.Enrollment
{
    public class EnrollmentResult
    {
        public bool Success { get; set; }
        public bool CanEnrollImmediately { get; set; }
        public string Message { get; set; }
        public int? EnrollmentId { get; set; }
    }
}

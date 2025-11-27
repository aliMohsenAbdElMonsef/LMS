namespace LMS.MVC.Models.ViewModels.Certificate
{
    public class CertificateViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string CertificateUrl { get; set; } = string.Empty;
    }
}



namespace LMS.MVC.Models.ViewModels.Enrollment
{
    
    public class EnrollmentManagementVM
    {
        public EnrollmentManagementRequest Request { get; set; } = new EnrollmentManagementRequest();
        public List<EnrollmentItemVM> Enrollments { get; set; } = new();
    }


}

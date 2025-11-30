using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Models.ViewModels.Course;

namespace LMS.MVC.Models.ViewModels.Home
{
    public class HomeViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalInstructors { get; set; }
        public List<ReadCategoryResult> Categories { get; set; } = new List<ReadCategoryResult>();
        public List<ReadCourseResult> PopularCourses { get; set; } = new List<ReadCourseResult>();
    }
}

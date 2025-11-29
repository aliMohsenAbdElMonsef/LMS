using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Models.ViewModels.Course;

namespace LMS.MVC.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<ReadCategoryResult> Categories { get; set; } = new List<ReadCategoryResult>();
        public List<ReadCourseResult> PopularCourses { get; set; } = new List<ReadCourseResult>();
    }
}
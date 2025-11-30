using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Course;

namespace LMS.MVC.Models.ViewModels.Home
{
    public class HomeViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalInstructors { get; set; }
        public IEnumerable<ReadCategoryDTO> Categories { get; set; } = new List<ReadCategoryDTO>();
        public IEnumerable<GetCourseDTO> PopularCourses { get; set; } = new List<GetCourseDTO>();
    }
}

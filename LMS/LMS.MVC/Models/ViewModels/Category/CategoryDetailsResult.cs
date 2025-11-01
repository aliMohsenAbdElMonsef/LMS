using LMS.MVC.Models.ViewModels.Course;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Category
{
    public class CategoryDetailsResult
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int CoursesCount { get; set; }
        public List<ReadCourseResult>Courses { get; set; } = new List<ReadCourseResult>();
    }
}

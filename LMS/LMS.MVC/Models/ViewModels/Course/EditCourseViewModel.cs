namespace LMS.MVC.Models.ViewModels.Course
{
    public class EditCourseViewModel: CreateCourseViewModel
    {
        public string Id { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? CategoryName { get; set; }
    }
}

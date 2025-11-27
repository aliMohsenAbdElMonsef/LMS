namespace LMS.MVC.Models.ViewModels.Lecture
{
    public class LectureViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class CreateLectureViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int Order { get; set; }
    }

    public class UpdateLectureViewModel : CreateLectureViewModel
    {
        public string Id { get; set; } = string.Empty;
    }
}

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
        public int LectureNumber { get; set; }
        public bool IsCompleted { get; set; }
        public string ZoomLink { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime LectureDate { get; set; }
        public TimeSpan StartTime { get; set; }
    }

    public class CreateLectureViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? VideoUrl { get; set; } = string.Empty;
        public string? Content { get; set; } = string.Empty;
        public int? DurationMinutes { get; set; }
        public int? LectureNumber { get; set; }
        public DateTime LectureDate { get; set; } = DateTime.UtcNow.Date;
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string InstructorId { get; set; } = string.Empty;
    }

    public class UpdateLectureViewModel : CreateLectureViewModel
    {
        public string Id { get; set; } = string.Empty;
    }
}

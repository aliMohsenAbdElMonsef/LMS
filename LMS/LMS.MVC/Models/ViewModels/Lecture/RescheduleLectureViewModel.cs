namespace LMS.MVC.Models.ViewModels.Lecture
{
    public class RescheduleLectureViewModel
    {
        public string LectureId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CurrentDate { get; set; }
        public TimeSpan CurrentStartTime { get; set; }
        public DateTime NewDate { get; set; }
        public TimeSpan NewStartTime { get; set; }
    }
}

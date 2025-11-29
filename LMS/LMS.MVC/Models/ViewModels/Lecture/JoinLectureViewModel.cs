using System;

namespace LMS.MVC.Models.ViewModels.Lecture
{
    public class JoinLectureViewModel
    {
        public string LectureId { get; set; }
        public string CourseName { get; set; }
        public string LectureTitle { get; set; }
        public DateTime LectureDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string InstructorName { get; set; }
        public string ZoomLink { get; set; }
        public bool IsActive { get; set; }
    }
}

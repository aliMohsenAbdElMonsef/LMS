using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Lecture
{
    public class LaunchLectureViewModel
    {
        public string LectureId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string LectureTitle { get; set; }
        public DateTime LectureDate { get; set; }
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Url]
        [Display(Name = "Zoom Meeting Link")]
        public string ZoomLink { get; set; }
    }
}

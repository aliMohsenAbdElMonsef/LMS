using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.LectureSchedule
{
    public class EditLectureScheduleViewModel
    {
        [Required]
        public string Id { get; set; }
        
        [Required]
        public string CourseId { get; set; }
        
        public string? CourseName { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }
        
        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Range(1, 480)]
        [Display(Name = "Duration (Minutes)")]
        public int DurationMinutes { get; set; }

        [Required]
        [Display(Name = "Instructor")]
        public string InstructorId { get; set; }
    }
}

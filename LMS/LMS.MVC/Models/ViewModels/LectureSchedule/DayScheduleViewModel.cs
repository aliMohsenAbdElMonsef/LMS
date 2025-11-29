using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.LectureSchedule
{
    public class DayScheduleViewModel
    {
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

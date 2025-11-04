using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Course
{
    public class DayScheduleViewModel
    {
        [Required(ErrorMessage = "Day of week is required")]
        [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Duration")]
        public TimeSpan Duration => EndTime - StartTime;

        [Display(Name = "Duration Hours")]
        public double DurationHours => Duration.TotalHours;
    }
}
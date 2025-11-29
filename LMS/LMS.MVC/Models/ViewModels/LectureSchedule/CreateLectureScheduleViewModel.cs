using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.LectureSchedule
{
    public class CreateLectureScheduleViewModel
    {
        [Required]
        public string CourseId { get; set; }
        
        public string CourseName { get; set; }
        
        [Required]
        [Display(Name = "Global Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public List<DayScheduleViewModel> Schedules { get; set; } = new List<DayScheduleViewModel>();
    }
}

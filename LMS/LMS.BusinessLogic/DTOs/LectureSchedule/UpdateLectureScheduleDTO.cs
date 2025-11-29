using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.LectureSchedule
{
    public class UpdateLectureScheduleDTO
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Range(1, 480)]
        public int DurationMinutes { get; set; }
    }
}

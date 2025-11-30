using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.LectureSchedule
{
    public class DayScheduleDTO
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(1, 480)]
        public int DurationMinutes { get; set; }

        [Required]
        public string InstructorId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.LectureSchedule
{
    public class CreateLectureScheduleDTO
    {
        [Required]
        public string CourseId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        
        [Required]
        public List<DayScheduleDTO> Schedules { get; set; } = new List<DayScheduleDTO>();
    }
}

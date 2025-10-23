using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Lecture
{
    public class CreateLectureDTO
    {
        [Required, MaxLength(150)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? ZoomLink { get; set; }

        public DateTime LectureDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [Required]
        public string CourseId { get; set; }

        public string? AssignedInstructorId { get; set; }

        public IFormFile? RecordingFile { get; set; }
    }
}

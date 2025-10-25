using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Lecture
{
    public class UpdateLectureDTO
    {
        [Required]
        public string Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // check if the lecture passed 
        // should be before the lecture date
        public string? AssignedInstructorId { get; set; }
        public DateTime? LectureDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? ZoomLink { get; set; }

        public string? RecordingPath { get; set; }

        // should be after the lecture date
        public string? lastUpdatedById { get; set; }
        // end
        public IFormFile? NewRecordingFile { get; set; }
    }
}

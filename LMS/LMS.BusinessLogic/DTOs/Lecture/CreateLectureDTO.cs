
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Lecture
{
    public class CreateLectureDTO
    {

        [Required, MaxLength(150)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        
        public DateTime LectureDate { get; set; }
        public TimeSpan StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }// calculated from the duration of the lecture in the course
        
        public int? DurationMinutes { get; set; }

        [Required]
        public string CourseId { get; set; }
        [Required]
        public string InstructorId { get; set; }
        public string? AssignedInstructorId { get; set; }

        public string? RecordingPath { get; set; }
    }
}

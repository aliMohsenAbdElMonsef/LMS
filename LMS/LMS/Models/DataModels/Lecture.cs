using System.ComponentModel.DataAnnotations;

namespace LMS.Models.DataModels
{
    public class Lecture
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime LectureDate { get; set; }

        // علاقة مع الكورس
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // علاقة مع الإنستركتور
        public string InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; }

        // لينك المحاضرة (Zoom أو غيره)
        public string? MeetingLink { get; set; }
    }
}

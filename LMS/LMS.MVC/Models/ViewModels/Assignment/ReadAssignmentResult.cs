using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Assignment
{
    public class ReadAssignmentResult
    {
        public string? Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public string CourseId { get; set; }
        public string? CourseName { get; set; }

        public string? InstructorId { get; set; }
        public string? InstructorName { get; set; }

        public DateTime UploadDate { get; set; }
        public int SubmissionsCount { get; set; }
    }
}
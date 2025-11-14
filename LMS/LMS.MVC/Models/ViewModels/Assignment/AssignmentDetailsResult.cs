using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Assignment
{
    public class AssignmentDetailsResult
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime DueDate { get; set; }

        [Required]
        public string CourseId { get; set; }
        public string CourseName { get; set; }

        [Required]
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }

        public int SubmissionsCount { get; set; }
        public List<StudentAssignmentResult> StudentSubmissions { get; set; } = new List<StudentAssignmentResult>();
    }
}
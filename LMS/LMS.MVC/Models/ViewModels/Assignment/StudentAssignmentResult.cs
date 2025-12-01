using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Assignment
{
    public class StudentAssignmentResult
    {
        public string Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        public string? StudentName { get; set; }

        [Required]
        public string AssignmentId { get; set; }

        public string? FilePath { get; set; }

        [Range(0, 100)]
        public double? Grade { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? GradedAt { get; set; }

        public string Status { get; set; }

        public string? Feedback { get; set; }

        // ✅ Added properties that the view needs
        public string? AssignmentTitle { get; set; }
        public string? CourseName { get; set; }
        public DateTime DueDate { get; set; }

        public string? StudentAssignmentId { get; set; }

        public bool IsSubmitted => !string.IsNullOrEmpty(FilePath);
        public bool IsGraded => Grade.HasValue;
        public bool IsPending => IsSubmitted && !IsGraded;
        public string? StatusDisplay { get; set; }

        private string GetStatusDisplay(string status)
        {
            return status switch
            {
                "NotSubmitted" => "Not Submitted",
                "PendingGrading" => "Pending Grading",
                "Graded" => "Graded",
                _ => status 
            };
        }
    }
}
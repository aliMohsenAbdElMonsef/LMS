using Domain.Enums;
using LMS.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class StudentAssignmentDTO
    {
        public string Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        public string StudentName { get; set; }

        [Required]
        public string AssignmentId { get; set; }

        public string? FilePath { get; set; }

        [Range(0, 100)]
        public double? Grade { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }

        // NEW: Added Status property with enum
        public AssignmentStatus Status { get; set; }

        // NEW: Added helper property for display
        public string StatusDisplay => GetStatusDisplay(Status);

        // NEW: Added feedback from instructor
        public string? Feedback { get; set; }

        // Helper method to get display text
        private string GetStatusDisplay(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.NotSubmitted => "Not Submitted",
                AssignmentStatus.PendingGrading => "Pending Grading",
                AssignmentStatus.Graded => "Graded",
                _ => "Unknown"
            };
        }
    }
}
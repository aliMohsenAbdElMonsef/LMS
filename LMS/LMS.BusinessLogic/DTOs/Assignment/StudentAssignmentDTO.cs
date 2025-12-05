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

        public string? StudentName { get; set; }

        [Required]
        public string AssignmentId { get; set; }

        public string? FilePath { get; set; }

        [Range(0, 100)]
        public double? Grade { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? GradedAt { get; set; }

        public AssignmentStatus Status { get; set; }

        public string? Feedback { get; set; }


        public string? AssignmentTitle { get; set; }
        public string? CourseName { get; set; }
        public DateTime DueDate { get; set; }

        public bool IsSubmitted => SubmittedAt != default(DateTime);


        public string StatusDisplay => GetStatusDisplay(Status);

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
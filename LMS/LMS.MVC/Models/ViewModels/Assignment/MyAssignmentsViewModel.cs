namespace LMS.MVC.Models.ViewModels.Assignment
{
    public class MyAssignmentsViewModel
    {
        public List<StudentAssignmentItemResult> Assignments { get; set; } = new();

        public int TotalAssignments => Assignments.Count;

        public int PendingAssignments => Assignments.Count(a => !a.IsSubmitted);

        public int CompletedAssignments => Assignments.Count(a => a.IsSubmitted);

        public int GradedCount => Assignments.Count(a => a.Grade.HasValue);

        public int OverdueCount => Assignments.Count(a => a.DueDate < DateTime.Now && !a.IsSubmitted);
    }

    public class StudentAssignmentItemResult
    {
        public string? AssignmentId { get; set; }
        public string? AssignmentTitle { get; set; }
        public string? CourseName { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public string? Status { get; set; }
        public string StatusDisplay { get; set; } = "Not Submitted";
        public double? Grade { get; set; }
        public string? FilePath { get; set; }
        public string? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
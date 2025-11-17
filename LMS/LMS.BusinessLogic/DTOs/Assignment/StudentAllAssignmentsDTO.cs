namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class StudentAllAssignmentsDTO
    {
        public string AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public string CourseName { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public string? Status { get; set; }
        public string StatusDisplay { get; set; }
        public double? Grade { get; set; }
        public string? FilePath { get; set; }
        public string? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
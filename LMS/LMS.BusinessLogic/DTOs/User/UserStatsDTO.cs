namespace LMS.BusinessLogic.DTOs.User
{
    public class UserStatsDTO
    {
        public int EnrolledCoursesCount { get; set; }
        public int CompletedCoursesCount { get; set; }
        public int CertificatesCount { get; set; }
        public int AssignmentsSubmitted { get; set; }
        public int QuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        

        public int CreatedCoursesCount { get; set; }
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
    }
}

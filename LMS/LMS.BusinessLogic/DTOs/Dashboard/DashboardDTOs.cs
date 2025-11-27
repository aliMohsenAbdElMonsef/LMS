using System.Collections.Generic;

namespace LMS.BusinessLogic.DTOs.Dashboard
{
    public class AdminDashboardDTO
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int PendingUsers { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalAssignments { get; set; }
        public double AverageStudentProgress { get; set; }
        public List<CourseStatsDTO> TopCourses { get; set; } = new List<CourseStatsDTO>();
    }

    public class InstructorDashboardDTO
    {
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalAssignments { get; set; }
        public double AverageStudentPerformance { get; set; }
        public List<CourseStatsDTO> MyCourses { get; set; } = new List<CourseStatsDTO>();
    }

    public class StudentDashboardDTO
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public int EnrolledCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int TotalQuizzesTaken { get; set; }
        public int TotalAssignmentsSubmitted { get; set; }
        public double AverageQuizScore { get; set; }
        public double AverageAssignmentScore { get; set; }
        public double OverallProgress { get; set; }
        public List<CourseProgressDTO> CourseProgress { get; set; } = new List<CourseProgressDTO>();
    }

    public class CourseStatsDTO
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public int EnrolledStudents { get; set; }
        public int CompletedStudents { get; set; }
        public double AverageProgress { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class CourseProgressDTO
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double Progress { get; set; }
        public int QuizzesTaken { get; set; }
        public int AssignmentsSubmitted { get; set; }
        public double AverageScore { get; set; }
    }
}

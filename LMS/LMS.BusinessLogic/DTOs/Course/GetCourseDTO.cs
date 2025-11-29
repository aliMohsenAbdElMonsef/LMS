using Domain.Enums;
using LMS.BusinessLogic.DTOs.DaySchedule;


namespace LMS.BusinessLogic.DTOs.Course
{
    public class GetCourseDTO
    {
        public string Id { get; set; }
        public string CourseCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public Level Level { get; set; }
        public string Language { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool EveryStuCouldEnroll { get; set; }
        public int DurationWeeks { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public Status Status { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public string? ThumbnailPath { get; set; }

        public string AdminId { get; set; }
        public string AdminName { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string CertificateTemplateId { get; set; }
        public string CertificateTemplateTitle { get; set; }
        public double MinAttendancePercentage { get; set; }
        public double MinPerformanceScore { get; set; }
        public bool AutoIssueCertificates { get; set; }
        public DateTime? LastUpdate { get; set; }

        public int EnrolledStudentsCount { get; set; }
        public double AverageRating { get; set; }

        public int TotalSessions { get; set; }
        public double HoursPerSession { get; set; }
        public int DaysPerWeek { get; set; }

        public List<LMS.BusinessLogic.DTOs.LectureSchedule.GetLectureScheduleDTO> Schedule { get; set; } = new();
        public List<InstructorInformationDTO> Instructors { get; set; } = new List<InstructorInformationDTO>();
        public List<LMS.BusinessLogic.DTOs.Assignment.ReadAssignmentDTO> Assignments { get; set; } = new List<LMS.BusinessLogic.DTOs.Assignment.ReadAssignmentDTO>();
        public List<LMS.BusinessLogic.DTOs.Quiz.ReadQuizDTO> Quizzes { get; set; } = new List<LMS.BusinessLogic.DTOs.Quiz.ReadQuizDTO>();
    }
}

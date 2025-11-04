using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.DaySchedule;
using Microsoft.EntityFrameworkCore.Query;

namespace LMS.MVC.Models.ViewModels.Course
{
    public class ReadCourseViewModel: EditCourseViewModel
    {
        public string AdminName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CertificateTemplateTitle { get; set; } = string.Empty;

        public int EnrolledStudentsCount { get; set; }
        public double AverageRating { get; set; }

        public int TotalSessions { get; set; }
        public double HoursPerSession { get; set; }
        public int DaysPerWeek { get; set; }

        public List<CreateDayScheduleDTO> Schedule { get; set; } = new(); // need update
        public List<InstructorInformationDTO> Instructors { get; set; } = new();// need update
        public DateTime? LastUpdate { get; set; }
    }
}

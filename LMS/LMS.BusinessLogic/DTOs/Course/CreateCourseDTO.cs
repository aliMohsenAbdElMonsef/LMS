using Domain.Enums;
using LMS.BusinessLogic.DTOs.DaySchedule;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class CreateCourseDTO
    {
        [MaxLength(100)]
        public string? CourseCode { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }
        [Required]
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;

        public int Credits { get; set; }

        public Level Level { get; set; }

        [Required]
        public string Language { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int DurationWeeks { get; set; }

        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;
        public Status Status { get; set; } = Status.Draft;

        public decimal Price { get; set; }

        public bool IsFree { get; set; } = false;

        public IFormFile? ThumbnailFile { get; set; }
        public string? ThumbnailPath { get; set; }

        [Required]
        public string AdminId { get; set; }

        [Required]
        public string CategoryId { get; set; }

        public string? CertificateTemplateID { get; set; }

        [Range(0, 100)]
        public double MinAttendancePercentage { get; set; } = 75;

        [Range(0, 100)]
        public double MinPerformanceScore { get; set; } = 60;

        public bool AutoIssueCertificates { get; set; } = false;

        [Range(1, 7)]
        public int DaysPerWeek { get; set; }

        
        [Range(0.5, 12)]
        public double HoursPerSession { get; set; }

        
        [Range(1, 1000)]
        public int TotalSessions { get; set; }

        
        public List<CreateDayScheduleDTO> DaySchedules { get; set; } = new List<CreateDayScheduleDTO>();

       
        public List<int> SelectedDays { get; set; } = new List<int>();
    }
}

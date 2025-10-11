using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class CreateCourseDTO
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public int Credits { get; set; }

        [Required]
        public string Language { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int DurationWeeks { get; set; }

        public decimal Price { get; set; }
        public bool IsFree { get; set; } = false;

        public IFormFile? ThumbnailFile { get; set; }

        [Required]
        public string AdminId { get; set; }

        [Required]
        public string CategoryId { get; set; }

        [Required]
        public string? CertificateTemplateId { get; set; }

        [Range(0, 100)]
        public double MinAttendancePercentage { get; set; } = 75;

        [Range(0, 100)]
        public double MinPerformanceScore { get; set; } = 60;

        public bool AutoIssueCertificates { get; set; } = false;
    }
}

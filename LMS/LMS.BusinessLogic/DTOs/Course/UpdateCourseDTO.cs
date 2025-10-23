using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class UpdateCourseDTO
    {
        [Required]
        public string Id { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int? Credits { get; set; }
        public string? Language { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;

        public int? DurationWeeks { get; set; }

        public decimal? Price { get; set; }
        public bool? IsFree { get; set; }

        public IFormFile? thumbnail { get; set; }

        public string? CategoryId { get; set; }
        public string? CertificateTemplateId { get; set; }

        [Range(0, 100)]
        public double? MinAttendancePercentage { get; set; }

        [Range(0, 100)]
        public double? MinPerformanceScore { get; set; }

        public bool? AutoIssueCertificates { get; set; }
    }
}

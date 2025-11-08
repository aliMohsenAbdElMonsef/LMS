using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class UpdateCourseDTO: IValidatableObject
    {
        [Required(ErrorMessage = "Course ID is required")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course name is required")]
        [MaxLength(200)]
        [Display(Name = "Course Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Description")]
        public string? Description { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Course Code")]
        public string? CourseCode { get; set; }

        [Required]
        public bool EveryStuCouldEnroll { get; set; }

        [Required]
        [Range(0, 300)]
        [Display(Name = "Credits")]
        public int Credits { get; set; }

        [Required]
        [Display(Name = "Level")]
        public Level Level { get; set; } = Level.Beginner;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Language")]
        public string Language { get; set; } = "English";

        // ================= DURATION =================
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7 + 84);

        [Display(Name = "Duration (Weeks)")]
        public int? DurationWeeks => CalculateTotalWeeks();

        // ================= MODE & STATUS =================
        [Required]
        [Display(Name = "Delivery Mode")]
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;

        [Display(Name = "Status")]
        public Status Status { get; set; } = Status.Draft;

        // ================= PRICE & CERTIFICATE =================
        [Range(0, 10000)]
        [Display(Name = "Price")]
        public decimal? Price { get; set; } = 0;

        [Display(Name = "Free Course")]
        public bool IsFree { get; set; } = false;

        [Display(Name = "Auto-Issue Certificates")]
        public bool AutoIssueCertificates { get; set; } = false;

        [Range(0, 100)]
        [Display(Name = "Minimum Attendance %")]
        public double MinAttendancePercentage { get; set; } = 75;

        [Range(0, 100)]
        [Display(Name = "Minimum Performance Score")]
        public double MinPerformanceScore { get; set; } = 60;

        [Display(Name = "Certificate Template")]
        public string? CertificateTemplateId { get; set; }

        // ================= MEDIA =================
        [Display(Name = "Course Thumbnail")]
        public IFormFile? ThumbnailFile { get; set; }

        // For internal use - stores the uploaded filename
        public string? ThumbnailFileName { get; set; }

        // ================= RELATIONS =================
        [Required]
        [Display(Name = "Category")]
        public string CategoryId { get; set; } = string.Empty;

        // Note: We don't include AdminId in UpdateDTO since it shouldn't change

        private int CalculateTotalWeeks()
        {
            if (StartDate == default || EndDate == default)
                return 0;

            var totalDays = (EndDate - StartDate).Days;
            return (int)Math.Ceiling(totalDays / 7.0);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
                yield return new ValidationResult("End date must be after start date.", new[] { nameof(EndDate) });

            if (StartDate < DateTime.Today)
                yield return new ValidationResult("Start date cannot be in the past.", new[] { nameof(StartDate) });

            if (DurationWeeks < 1)
                yield return new ValidationResult("Course duration must be at least 1 week.", new[] { nameof(EndDate) });

            if (DurationWeeks > 52)
                yield return new ValidationResult("Course duration cannot exceed 52 weeks.", new[] { nameof(EndDate) });
        }
    }
}
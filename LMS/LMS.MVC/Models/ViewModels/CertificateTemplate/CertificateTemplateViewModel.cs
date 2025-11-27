using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.CertificateTemplate
{
    public class CertificateTemplateViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MessageBody { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string AdminId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCertificateTemplateViewModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        
        [Required]
        public string MessageBody { get; set; } = "This certificate is presented to {StudentName} for successfully completing the course \"{CourseName}\".";
        
        [Required]
        public string CourseId { get; set; }

        public string? AdminId { get; set; }
    }

    public class UpdateCertificateTemplateViewModel
    {
        [Required]
        public string Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        
        [Required]
        public string MessageBody { get; set; }
    }
}

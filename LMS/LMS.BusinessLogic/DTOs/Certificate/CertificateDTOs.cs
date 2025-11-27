using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Certificate
{
    public class GenerateCertificateDTO
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public string CourseId { get; set; }
    }

    public class ReadStudentCertificateDTO
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string CertificateTemplateId { get; set; }
        public string GeneratedPath { get; set; }
        public DateTime IssuedDate { get; set; }
    }

    public class CertificateDataDTO
    {
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public DateTime CompletionDate { get; set; }
        public string InstructorName { get; set; }
        public string CertificateTitle { get; set; }
        public string CertificateMessage { get; set; }
    }
}

using Domain.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.MainEntities
{
    public class CertificateTemplate : SoftDeletion
    {
        public string Id { get; set; }

        public CertificateTemplate()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Required, MaxLength(150)]
        public string Title { get; set; } = "Certificate of Completion";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(3000)]
        public string? MessageBody { get; set; } =
            "This certificate is presented to {StudentName} for successfully completing the course \"{CourseName}\" with outstanding performance.";


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AdminId { get; set; }
        public ApplicationUser Admin { get; set; }

        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        public ICollection<StudentCertificate> StudentCertificates { get; set; } = new List<StudentCertificate>();
    }
}

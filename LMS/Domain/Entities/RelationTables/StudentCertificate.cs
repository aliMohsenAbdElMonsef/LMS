using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class StudentCertificate: SoftDeletion
    {
        public string Id { get; set; }
        public StudentCertificate()
        {
            Id = Guid.NewGuid().ToString();
        }

        [MaxLength(500)]
        public string GeneratedPath { get; set; }  

        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        // relation
        // user
            // student
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student;
        // certificate template
        [Required]
        public string certificateTamplateId;
        public CertificateTemplate CertificateTemplate { get; set; }
    }
}

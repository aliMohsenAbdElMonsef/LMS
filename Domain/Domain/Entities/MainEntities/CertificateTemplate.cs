using Domain.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class CertificateTemplate : SoftDeletion
    {
        public string Id { get; set; }
        public CertificateTemplate()
        {
            Id = Guid.NewGuid().ToString();
        }
        [MaxLength(500)]
        [Required]
        public string TemplatePath { get; set; }

        [MaxLength(2000)]
        public string? TemplateContent { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // relations
        // user
        // admin
        [Required]
        public string AdminId {  get; set; }

        public ApplicationUser Admin { get; set; }

        // course
        [Required]
        public string CourseId { get; set; }

        public Course Course { get; set; }

        // student certificate
        public ICollection<StudentCertificate> studentCertificates { get; set; }=new List<StudentCertificate>();
    }
}

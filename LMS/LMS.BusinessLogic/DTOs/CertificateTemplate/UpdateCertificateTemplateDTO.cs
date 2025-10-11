using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.CertificateTemplate
{
    public class UpdateCertificateTemplateDTO
    {
        [Required]
        public string Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

    }
}

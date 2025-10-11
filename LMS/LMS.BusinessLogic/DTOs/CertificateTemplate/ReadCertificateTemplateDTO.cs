using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.CertificateTemplate
{
    public class ReadCertificateTemplateDTO
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public string? MessageBody { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CourseId { get; set; }

        public string CourseName { get; set; }

        public string AdminId { get; set; }

        public string AdminName { get; set; }
    }
}

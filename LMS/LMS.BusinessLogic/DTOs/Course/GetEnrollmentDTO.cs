using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class GetEnrollmentDTO
    {
        public string? studentId { get; set; }

        public string? courseId { get; set; } = string.Empty;
    }
}

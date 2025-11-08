using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Enrollment
{
    public class FilterEnrollmentDto
    {
        public string? Status { get; set; }
        public string? UserSearch { get; set; }
        public string? CourseCode { get; set; }
        public string? Role { get; set; }
    }

}

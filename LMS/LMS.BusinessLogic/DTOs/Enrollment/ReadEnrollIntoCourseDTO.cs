using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Enrollment
{
    public class ReadEnrollIntoCourseDTO
    {
       
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public string CourseCode {  get; set; } = string.Empty;

        public string Role { get; set; }
        public string UserEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }
    }

}

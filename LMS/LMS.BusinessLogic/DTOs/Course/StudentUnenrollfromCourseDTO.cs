using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class StudentUnenrollfromCourseDTO
    {
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class ReadStudentEnrollIntoCourseDTO
    {
       
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public double Progress { get; set; }
    }
}

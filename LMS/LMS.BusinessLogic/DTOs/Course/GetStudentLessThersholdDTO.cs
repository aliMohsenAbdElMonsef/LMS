using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class GetStudentLessThersholdDTO
    {
        public string courseId { get; set; }
        public double threshold { get; set; }
    }
}

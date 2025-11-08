using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Enrollment
{
    public class ReadStudentEnrollmentDTO: ReadEnrollIntoCourseDTO
    {

        public double Progress { get; set; }
    }
}

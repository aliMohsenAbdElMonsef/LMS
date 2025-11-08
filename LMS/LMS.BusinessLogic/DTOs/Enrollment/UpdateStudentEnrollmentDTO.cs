using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Enrollment
{
    public class UpdateStudentEnrollmentDTO: UpdateEnrollIntoCourseDTO
    {
        public double Progress { get; set; }
    }
}

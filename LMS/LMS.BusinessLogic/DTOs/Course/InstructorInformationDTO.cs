using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class InstructorInformationDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<string> AssignedDays { get; set; } = new List<string>();
    }
}

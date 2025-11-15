using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class SubmitAssignmentDTO
    {
        [Required]
        public string AssignmentId { get; set; }

        [Required]
        public string StudentId { get; set; }

        
        public string? FilePath { get; set; }
    }
}

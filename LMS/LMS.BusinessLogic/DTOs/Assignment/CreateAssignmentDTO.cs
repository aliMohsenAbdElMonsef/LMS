using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class CreateAssignmentDTO
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public string? Description { get; set; }

        
        public string? FilePath { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public string CourseId { get; set; }

        [Required]
        public string InstructorId { get; set; }
    }
}

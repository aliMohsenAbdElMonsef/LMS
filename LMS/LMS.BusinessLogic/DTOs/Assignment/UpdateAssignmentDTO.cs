using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class UpdateAssignmentDTO
    {
        [Required]
        public string Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public IFormFile? File { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public string InstructorId { get; set; }

        [Required]
        public string CourseId { get; set; }
    }
}

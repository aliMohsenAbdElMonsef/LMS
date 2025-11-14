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
        public string Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public DateTime? DueDate { get; set; }
    }

   
}

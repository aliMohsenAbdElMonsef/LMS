using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class AssignmentDetailsDTO
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime DueDate { get; set; }

        [Required]
        public string CourseId { get; set; }
        public string CourseName { get; set; }

        [Required]
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }

        public int SubmissionsCount { get; set; }
        public List<StudentAssignmentDTO> StudentSubmissions { get; set; } = new List<StudentAssignmentDTO>();
    }
}

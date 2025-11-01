using LMS.BusinessLogic.DTOs.Course;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Category
{
    public class CategoryDetailsDTO
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int CoursesCount { get; set; }

        public List<GetCourseDTO> Courses { get; set; } = new List<GetCourseDTO>();
    }
}

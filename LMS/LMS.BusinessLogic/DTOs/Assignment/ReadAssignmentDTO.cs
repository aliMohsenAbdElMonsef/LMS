using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Assignment
{
    public class ReadAssignmentDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime DueDate { get; set; }
        public string InstructorId { get; set; }
        public string? InstructorName { get; set; }   
        public string CourseId { get; set; }
        public string? CourseName { get; set; }       
        public int NumberofSubmissions { get; set; }        
    }
}

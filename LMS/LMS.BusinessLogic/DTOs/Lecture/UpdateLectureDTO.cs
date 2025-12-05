using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Lecture
{
    public class UpdateLectureDTO
    {
        [Required]
        public string Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

 

        public string? AssignedInstructorId { get; set; }
        public DateTime? LectureDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? ZoomLink { get; set; }

        public string? RecordingPath { get; set; }


        public string? lastUpdatedById { get; set; }

        public IFormFile? NewRecordingFile { get; set; }
        public string? MaterialsPath { get; set; }
        public IFormFile? NewMaterialsFile { get; set; }
    }
}

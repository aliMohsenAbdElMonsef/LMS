using Microsoft.AspNetCore.Http;

namespace LMS.MVC.Models.ViewModels.Lecture
{
    public class UploadLectureContentViewModel
    {
        public string LectureId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string LectureTitle { get; set; } = string.Empty;
        public string? CurrentRecordingPath { get; set; }
        public string? CurrentMaterialsPath { get; set; }
        public IFormFile? NewRecordingFile { get; set; }
        public IFormFile? NewMaterialsFile { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace LMS.MVC.Models.ViewModels.Enrollment
{
    public class EnrollmentModel
    {
        [JsonPropertyName("courseId")]
        public string CourseId { get; set; }
    }
}

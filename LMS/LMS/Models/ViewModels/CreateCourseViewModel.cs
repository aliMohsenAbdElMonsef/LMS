using LMS.Models.DataModels;
using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels
{
    public class CreateCourseViewModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? CourseCode { get; set; }

        [Required, MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public int Credits { get; set; }

        [Required]
        public Levels Level { get; set; }

        [Required, MaxLength(50)]
        public string Language { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        public int DurationWeeks { get; set; }

        [Required]
        public decimal Price { get; set; }
        public bool IsFree { get; set; }

        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [Required]
        public int CategoryId { get; set; }
        public List<Category>? AllCategories { get; set; } = new();

        public IFormFile? Thumbnail { get; set; }

        public List<int> SelectedSkillIds { get; set; } = new();
        public List<Skill>? AllSkills { get; set; } = new();

        public List<int> SelectedPrerequisiteIds { get; set; } = new();
        public List<Course>? AllCourses { get; set; } = new();
    }
}

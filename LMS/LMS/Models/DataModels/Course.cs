using System.ComponentModel.DataAnnotations;

namespace LMS.Models.DataModels
{
    public enum Levels { Beginner, Intermediate, Advanced }
    public enum DeliveryMode { Online, Onsite, Hybrid }
    public enum CourseStatus { Draft, Published, Ongoing, Completed, Archived }

    public class Course
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? CourseCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public int Credits { get; set; }

        public Levels Level { get; set; }

        [Required]
        [MaxLength(50)]
        public string Language { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int DurationWeeks { get; set; }

        public int EnrolledCount { get; set; } = 0;

        public List<CourseInstructor> CourseInstructors { get; set; } = new();

        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        public decimal Price { get; set; }
        public bool IsFree { get; set; }

        public double Rating { get; set; }
        public int RatingCount { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        public List<CourseSkill> Skills { get; set; } = new();

        public List<Assignment> Assignments { get; set; } = new();

        public List<CoursePrerequisite> Prerequisites { get; set; } = new();

        public List<CoursePrerequisite> IsPrerequisiteFor { get; set; } = new();
    }
}

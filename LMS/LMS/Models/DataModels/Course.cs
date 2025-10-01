namespace LMS.Models.DataModels
{
    public enum Levels { Beginner, Intermediate, Advanced }
    public enum DeliveryMode { Online, Onsite, Hybrid }
    public enum CourseStatus { Draft, Published, Ongoing, Completed, Archived }

    public class Course
    {
        public int CourseId { get; set; }

        public string CourseCode { get; set; } = string.Empty; 
        public string Title { get; set; }
        public string Description { get; set; }

        public int Credits { get; set; }
        public Levels Level { get; set; }
        public string Language { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DurationWeeks { get; set; }

        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }


        public List<CourseInstructor> courseInstructors { get; set; } = new();
        
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public decimal Price { get; set; }
        public bool IsFree { get; set; }

        
        public double Rating { get; set; } 
        public int RatingCount { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

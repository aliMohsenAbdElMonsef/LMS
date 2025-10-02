namespace LMS.Models.DataModels
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    }
}

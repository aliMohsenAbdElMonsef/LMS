namespace LMS.Models.DataModels
{
    public class Enrollment
    {
        public int Id { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.Now;
    }
}

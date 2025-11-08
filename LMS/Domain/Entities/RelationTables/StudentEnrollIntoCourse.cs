using Domain.Entities.MainEntities;
using Domain.Enums;


namespace Domain.Entities.RelationTables
{
    public class StudentEnrollIntoCourse: SoftDeletion
    {
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student;

        public string CourseId { get; set; } = string.Empty;
        public Course Course;

        public double progress { get; set; } = 0.0;
        public DateTime CreatedAt { get; set; }
        public ApplicationStatus Status { get; set; }
    }
}

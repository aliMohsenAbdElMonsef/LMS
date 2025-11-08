using Domain.Entities;
using Domain.Entities.MainEntities;
using Domain.Enums;

namespace LMS.Entity.Entities.RelationTables
{
    public class InstructorEnrolltoCourse: SoftDeletion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; }
        public string CourseId { get; set; }
        public Course Course { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }

        

    }
}

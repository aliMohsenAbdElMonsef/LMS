using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.Entity.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class StudentAssignment : SoftDeletion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        [Required]
        public string AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public double? Grade { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? GradedAt { get; set; }

        // NEW: Added AssignmentStatus Enum
        public AssignmentStatus Status { get; set; } = AssignmentStatus.NotSubmitted;

        [MaxLength(1000)]
        public string? Feedback { get; set; }
    }
}
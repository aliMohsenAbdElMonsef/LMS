using Domain.Entities;
using Domain.Entities.MainEntities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

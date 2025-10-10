using Domain.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.MainEntities
{
    public class Lecture : SoftDeletion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? RecordingPath { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartHour { get; set; }
        public TimeSpan? EndHour { get; set; }

        [MaxLength(500)]
        public string? ZoomLink { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedAt { get; set; }

        public DateTime? LastRecordingUploadDate { get; set; }
        // relations
        // course
        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        // user
        // instructor
        public string? AssignedInstructorId { get; set; }
        public ApplicationUser? AssignedInstructor { get; set; }

        public string? LastUploadedByInstructorId { get; set; }
        public ApplicationUser? LastUploadedByInstructor { get; set; }
        // student
        public ICollection<StudentLecture> Students { get; set; } = new List<StudentLecture>();

    }
}

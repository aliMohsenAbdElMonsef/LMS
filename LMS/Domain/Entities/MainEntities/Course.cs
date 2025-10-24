using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.Entity.Entities.MainEntities;
using LMS.Entity.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class Course : SoftDeletion
    {
        public string Id { get; set; }
        public Course() { 
            Id = Guid.NewGuid().ToString();
        }
        [MaxLength(100)]
        public string? CourseCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int? Credits { get; set; }

        public Level Level { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Language { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int? DurationWeeks { get; set; }


        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;
        public Status Status { get; set; } = Status.Draft;

        public decimal? Price { get; set; }
        public bool? IsFree { get; set; }

        public DateTime? LastUpdate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? ThumbnailPath { get; set; }

        public bool? AutoIssueCertificates { get; set; } = false;

        // course post requirements

        [Range(0, 100)]
        public double? MinPerformanceScore { get; set; } = 60;

        [Range(0, 100)]
        public double? MinAttendancePercentage { get; set; } = 75;

        // Relations
        // user
        // instructor
        public ICollection<InstructorEnrolltoCourse> InstructorEnrollments { get; set; }
        // student
        public ICollection<StudentEnrollIntoCourse> Students { get; set; }= new List<StudentEnrollIntoCourse>();
        public ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
        // admin
        [Required]
        public string AdminId { get; set; }
        public ApplicationUser Admin { get; set; }
        // certificate template
        public string? CertificateTemplateID { get; set; }

        public CertificateTemplate? CertificateTemplate { get; set; }

        //assignment
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        
        // category
        [Required]
        public string CategoryId { get; set; }
        public Category Category { get; set; }

        //lecture
        public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

        // lecture schedulling 
        public ICollection<LectureSchedule> LectureSchedules { get; set; } = new List<LectureSchedule>();
        // quiz
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        // course skills
        public ICollection<CourseSkill> Skills { get; set; } = new List<CourseSkill>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Lecture
{   
        public class ReadLectureDTO
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string? Description { get; set; }
            public string? RecordingPath { get; set; }
            public string? ZoomLink { get; set; }
            
            public DateTime CreatedAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public DateTime? LastRecordingUploadDate { get; set; }
            public DateTime LectureDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int DurationMinutes { get; set; }

            public string CourseId { get; set; }
            public string CourseName { get; set; }

            public string? AssignedInstructorId { get; set; }
            public string? AssignedInstructorName { get; set; }
            
            public string? UploadedById { get; set; }
            public string? UploadedByName { get; set; }
        public int? NumberofAttendedStudents { get; set; }
        }
}



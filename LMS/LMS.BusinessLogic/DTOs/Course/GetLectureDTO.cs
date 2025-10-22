using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Course
{
    public class GetLectureDTO
    {
        public string Id { get; set; }

        public int LectureNumber { get; set; }

        public string Title { get; set; }

        public DateOnly ScheduleDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string? ZoomLink { get; set; }

        public string? RecordingPath { get; set; }

        public InstructorInformationDTO Instructor { get; set; }
    }
}

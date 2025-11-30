using System;

namespace LMS.BusinessLogic.DTOs.LectureSchedule
{
    public class GetLectureScheduleDTO
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public int GeneratedLecturesCount { get; set; }
        public TimeSpan EndTime => StartTime.Add(TimeSpan.FromMinutes(DurationMinutes));
    }
}

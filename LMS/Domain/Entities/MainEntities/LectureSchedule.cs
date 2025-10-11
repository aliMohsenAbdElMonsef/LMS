using Domain.Entities;
using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Entity.Entities.MainEntities
{
    public class LectureSchedule: SoftDeletion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }

        public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
    }

}

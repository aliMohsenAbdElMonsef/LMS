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
    public class CourseDaySchedule : SoftDeletion
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [Range(0,6)]
        public int DayOfWeek { get; set; }

        public TimeSpan StartaTime { get; set; }
        public TimeSpan EndTime { get; set; }

    }
}

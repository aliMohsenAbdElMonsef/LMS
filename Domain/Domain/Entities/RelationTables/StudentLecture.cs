using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class StudentLecture
    {
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; }
        public string LectureId { get; set; } = string.Empty;
        public Lecture Lecture { get; set; }
        public bool IsAttended { get; set; } = false;
        public DateTime? AttendanceDate { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models.DataModels
{
    public class skill
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string LecturesHours { get; set; }   // e.g. "5 lectures (3h)"
        
        public int CourseId { get; set; }
        public virtual course Course { get; set; }

    }
}

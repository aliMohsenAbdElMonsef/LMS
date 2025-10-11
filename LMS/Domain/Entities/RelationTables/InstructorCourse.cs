using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class InstructorCourse
    {
        public string CourseId { get; set; } = string.Empty;
        public Course Course;

        public string InstructorId { get; set; } = string.Empty;
        public ApplicationUser Instructor { get; set; }
    }
}
